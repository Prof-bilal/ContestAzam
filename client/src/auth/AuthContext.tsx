import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import type { UserDto } from "../types";
import * as api from "../api/client";

type Status = "loading" | "authenticated" | "unauthenticated";

interface AuthContextValue {
  user: UserDto | null;
  status: Status;
  login: (email: string, password: string) => Promise<void>;
  register: (
    name: string,
    email: string,
    password: string,
    confirmPassword: string,
    accountType?: "Visitor" | "Organizer",
    organizationName?: string,
    organizationReason?: string,
    organizationExperience?: string,
  ) => Promise<"ok" | "emailVerificationRequired">;
  logout: () => Promise<void>;
  restoreSession: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [status, setStatus] = useState<Status>("loading");

  const restoreSession = useCallback(async () => {
    const restored = await api.bootstrap();
    setUser(restored);
    setStatus(restored ? "authenticated" : "unauthenticated");
    return restored !== null;
  }, []);

  useEffect(() => {
    void restoreSession();
  }, [restoreSession]);

  const login = useCallback(async (email: string, password: string) => {
    const u = await api.login(email, password);
    setUser(u);
    setStatus("authenticated");
  }, []);

  const register = useCallback(
    async (
      name: string,
      email: string,
      password: string,
      confirmPassword: string,
      accountType: "Visitor" | "Organizer" = "Visitor",
      organizationName?: string,
      organizationReason?: string,
      organizationExperience?: string,
    ) => {
      const u = await api.register(
        name,
        email,
        password,
        confirmPassword,
        accountType,
        organizationName,
        organizationReason,
        organizationExperience,
      );
      setUser(u);
      setStatus("authenticated");
      return "ok" as const;
    },
    [],
  );

  const logout = useCallback(async () => {
    await api.logout();
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, status, login, register, logout, restoreSession }),
    [user, status, login, register, logout, restoreSession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
