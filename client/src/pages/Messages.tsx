import { useCallback, useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import * as api from "../api/client";
import type { ConversationDto, ConversationDetailDto } from "../types";
import { useRealtime } from "../realtime/RealtimeContext";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { LogoutButton } from "../components/LogoutButton";

const MAX_LENGTH = 2000;
export function Messages() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const { onMessage, connected } = useRealtime();

  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeId, setActiveId] = useState<number | null>(null);
  const [detail, setDetail] = useState<ConversationDetailDto | null>(null);
  const [draft, setDraft] = useState("");
  const [sending, setSending] = useState(false);
  const [loadingList, setLoadingList] = useState(true);
  const [loadingThread, setLoadingThread] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [newRecipient, setNewRecipient] = useState("");
  const [creating, setCreating] = useState(false);

  const bottomRef = useRef<HTMLDivElement | null>(null);

  const loadConversations = useCallback(async () => {
    setLoadingList(true);
    try {
      const list = await api.getConversations();
      setConversations(list);
    } catch {
      setError("Unable to load conversations.");
    } finally {
      setLoadingList(false);
    }
  }, []);

  useEffect(() => {
    void loadConversations();
  }, [loadConversations]);

  const openConversation = useCallback(async (id: number) => {
    setActiveId(id);
    setLoadingThread(true);
    setError(null);
    try {
      const d = await api.getConversation(id);
      setDetail(d);
      await api.markConversationRead(id);
      setConversations((prev) =>
        prev.map((c) => (c.id === id ? { ...c, unreadCount: 0 } : c)),
      );
    } catch (e) {
      if (e instanceof Error && e.name === "ApiError") setError("That conversation is not available.");
      else setError("Unable to load the conversation.");
      setDetail(null);
    } finally {
      setLoadingThread(false);
    }
  }, []);

  // Real-time incoming messages.
  useEffect(
    () =>
      onMessage((incoming) => {
        if (activeId !== null && incoming.conversationId === activeId) {
          setDetail((prev) =>
            prev
              ? { ...prev, messages: [...prev.messages.filter((m) => m.id !== incoming.id), incoming] }
              : prev,
          );
          void api.markConversationRead(incoming.conversationId).catch(() => undefined);
        }
        setConversations((prev) => {
          const idx = prev.findIndex((c) => c.id === incoming.conversationId);
          if (idx === -1) {
            void loadConversations();
            return prev;
          }
          const updated = [...prev];
          updated[idx] = {
            ...updated[idx],
            lastMessage: incoming.content,
            lastMessageAt: incoming.sentAt,
            unreadCount:
              activeId === incoming.conversationId ? 0 : updated[idx].unreadCount + 1,
          };
          return updated.sort(
            (a, b) => new Date(b.lastMessageAt ?? b.updatedAt).getTime() - new Date(a.lastMessageAt ?? a.updatedAt).getTime(),
          );
        });
      }),
    [onMessage, activeId, loadConversations],
  );

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [detail?.messages.length]);

  const send = async (e: React.FormEvent) => {
    e.preventDefault();
    const content = draft.trim();
    if (!content || activeId === null || sending) return;
    if (content.length > MAX_LENGTH) {
      addToast("error", `Message must be at most ${MAX_LENGTH} characters.`);
      return;
    }

    setSending(true);
    try {
      const saved = await api.sendMessage(activeId, content);
      setDetail((prev) =>
        prev ? { ...prev, messages: [...prev.messages.filter((m) => m.id !== saved.id), saved] } : prev,
      );
      setConversations((prev) =>
        prev.map((c) =>
          c.id === activeId
            ? { ...c, lastMessage: saved.content, lastMessageAt: saved.sentAt }
            : c,
        ),
      );
      setDraft("");
    } catch {
      addToast("error", "Unable to send message.");
    } finally {
      setSending(false);
    }
  };

  const startConversation = async (e: React.FormEvent) => {
    e.preventDefault();
    const id = Number(newRecipient);
    if (!Number.isInteger(id) || id <= 0) {
      addToast("error", "Enter a valid user ID.");
      return;
    }
    if (id === user?.id) {
      addToast("error", "You cannot message yourself.");
      return;
    }
    if (creating) return;
    setCreating(true);
    try {
      const conv = await api.createConversation(id);
      await loadConversations();
      setActiveId(conv.id);
      setDetail(conv);
      setNewRecipient("");
      addToast("success", "Conversation ready.");
    } catch {
      addToast("error", "Could not start that conversation.");
    } finally {
      setCreating(false);
    }
  };

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/notifications" className="admin-nav-item">Notifications</Link>
          <Link to="/messages" className="admin-nav-item active">Messages</Link>
          <Link to="/profile" className="admin-nav-item">Profile</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      <main className="admin-main">
        <header className="admin-header notif-page-header">
          <h1>Messages</h1>
          <span className={`conn-pill ${connected ? "on" : "off"}`}>
            {connected ? "Live" : "Reconnecting…"}
          </span>
        </header>

        {error && (
          <div className="card error-state" role="alert">
            {error}
            <button type="button" className="btn btn-small" onClick={() => void loadConversations()}>
              Retry
            </button>
          </div>
        )}

        <form className="new-conversation card" onSubmit={startConversation}>
          <label htmlFor="recipient">Start a conversation by user ID</label>
          <div style={{ display: "flex", gap: "0.5rem" }}>
            <input
              id="recipient"
              type="number"
              min={1}
              value={newRecipient}
              onChange={(e) => setNewRecipient(e.target.value)}
              placeholder="User ID"
            />
            <button className="btn btn-small" type="submit" disabled={creating || !newRecipient}>
              {creating ? "Starting…" : "Start"}
            </button>
          </div>
        </form>

        <div className="messages-layout">
          {/* Conversation list */}
          <section className="conversation-list card" aria-label="Conversations">
            {loadingList && <div className="loading-state">Loading conversations…</div>}
            {!loadingList && conversations.length === 0 && (
              <div className="empty-state">No conversations yet.</div>
            )}
            {conversations.map((c) => (
              <button
                key={c.id}
                type="button"
                className={`conversation-row ${activeId === c.id ? "active" : ""}`}
                onClick={() => void openConversation(c.id)}
              >
                <span className="conversation-name">{c.otherUserName}</span>
                <span className="conversation-last">{c.lastMessage ?? "No messages yet"}</span>
                <span className="conversation-meta">
                  {c.lastMessageAt && new Date(c.lastMessageAt).toLocaleString()}
                </span>
                {c.unreadCount > 0 && <span className="notif-badge">{c.unreadCount}</span>}
              </button>
            ))}
          </section>

          {/* Chat window */}
          <section className="chat-window card" aria-label="Chat">
            {activeId === null && !loadingThread && (
              <div className="empty-state">Select a conversation to start chatting.</div>
            )}
            {loadingThread && <div className="loading-state">Loading messages…</div>}

            {detail && activeId !== null && !loadingThread && (
              <>
                <div className="chat-header">
                  <strong>{detail.otherUserName}</strong>
                </div>
                <div className="chat-messages">
                  {detail.messages.length === 0 && (
                    <div className="empty-state">No messages yet — say hello!</div>
                  )}
                  {detail.messages.map((m) => {
                    const mine = m.senderId === user?.id;
                    return (
                      // Content is rendered as a plain React text node — never raw HTML.
                      <div key={m.id} className={`bubble ${mine ? "mine" : "theirs"}`}>
                        <p>{m.content}</p>
                        <time dateTime={m.sentAt}>
                          {new Date(m.sentAt).toLocaleTimeString()}
                          {mine && (m.isRead ? " · Read" : " · Sent")}
                        </time>
                      </div>
                    );
                  })}
                  <div ref={bottomRef} />
                </div>
                <form className="chat-composer" onSubmit={send}>
                  <input
                    value={draft}
                    maxLength={MAX_LENGTH}
                    onChange={(e) => setDraft(e.target.value)}
                    placeholder="Type a message…"
                    aria-label="Message"
                  />
                  <button type="submit" disabled={sending || draft.trim().length === 0}>
                    {sending ? "Sending…" : "Send"}
                  </button>
                </form>
              </>
            )}
          </section>
        </div>
      </main>
    </div>
  );
}