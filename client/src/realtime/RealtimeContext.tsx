import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import type { ReactNode } from "react";
import * as signalR from "@microsoft/signalr";
import { getAccessToken } from "../api/client";
import { useAuth } from "../auth/AuthContext";

export interface RealtimeMessage {
  id: number;
  conversationId: number;
  senderId: number;
  content: string;
  sentAt: string;
  isRead: boolean;
  readAt: string | null;
}

/**
 * Single, app-wide SignalR connection for both notifications and messaging.
 *
 * Lifecycle:
 *   - authenticated → connect (once; re-renders never create a second connection)
 *   - logout / token loss → stop and clear handlers
 *   - transient network failure → automatic reconnect with backoff
 */
export interface RealtimeContextValue {
  connected: boolean;
  /** Subscribe to server-pushed notifications. Returns an unsubscribe fn. */
  onNotification: (handler: () => void) => () => void;
  /** Subscribe to incoming chat messages. Returns an unsubscribe fn. */
  onMessage: (handler: (message: RealtimeMessage) => void) => () => void;
}

const RealtimeContext = createContext<RealtimeContextValue | undefined>(undefined);

type NotificationHandler = () => void;
type MessageHandler = (message: RealtimeMessage) => void;

export function RealtimeProvider({ children }: { children: ReactNode }) {
  const { status } = useAuth();
  const [connected, setConnected] = useState(false);

  const notificationHandlers = useRef(new Set<NotificationHandler>());
  const messageHandlers = useRef(new Set<MessageHandler>());
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const onNotification = useCallback((handler: NotificationHandler) => {
    notificationHandlers.current.add(handler);
    return () => {
      notificationHandlers.current.delete(handler);
    };
  }, []);

  const onMessage = useCallback((handler: MessageHandler) => {
    messageHandlers.current.add(handler);
    return () => {
      messageHandlers.current.delete(handler);
    };
  }, []);

  useEffect(() => {
    if (status !== "authenticated") {
      // Logout / session loss: tear the connection down so no private data flows.
      const conn = connectionRef.current;
      connectionRef.current = null;
      setConnected(false);
      notificationHandlers.current.clear();
      messageHandlers.current.clear();
      if (conn) {
        void conn.stop().catch(() => undefined);
      }
      return;
    }

    // Already connected or connecting — prevents duplicate connections.
    if (connectionRef.current) return;

    const buildConnection = (): signalR.HubConnection =>
      new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications", {
          accessTokenFactory: () => getAccessToken() ?? "",
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    const conn = buildConnection();

    conn.on("NotificationReceived", () => {
      notificationHandlers.current.forEach((h) => h());
    });

    conn.on("MessageReceived", (message: Parameters<MessageHandler>[0]) => {
      messageHandlers.current.forEach((h) => h(message));
    });

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => setConnected(true));
    conn.onclose(() => {
      setConnected(false);
      // Only clear our own handle.
      if (connectionRef.current === conn) connectionRef.current = null;
    });

    connectionRef.current = conn;

    void conn
      .start()
      .then(() => setConnected(true))
      .catch(() => {
        // Server may be temporarily unreachable; automatic reconnect will retry.
        setConnected(false);
      });

    return () => {
      // StrictMode double-invokes effects in dev; keep a single live connection.
      void conn.stop().catch(() => undefined);
      if (connectionRef.current === conn) connectionRef.current = null;
    };
  }, [status]);

  const value = useMemo(
    () => ({ connected, onNotification, onMessage }),
    [connected, onNotification, onMessage],
  );

  return <RealtimeContext.Provider value={value}>{children}</RealtimeContext.Provider>;
}

export function useRealtime(): RealtimeContextValue {
  const ctx = useContext(RealtimeContext);
  if (!ctx) throw new Error("useRealtime must be used within RealtimeProvider");
  return ctx;
}