import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { ApiError, NetworkError, completeOAuthRegistration, uploadProfileImage } from "../api/client";

function containsEmoji(text: string): boolean {
  for (const char of text) {
    const code = char.codePointAt(0)!;
    if (code >= 0x10000) return true;
    if (
      (code >= 0x2600 && code <= 0x26FF) ||
      (code >= 0x2700 && code <= 0x27BF) ||
      (code >= 0x2300 && code <= 0x23FF) ||
      (code >= 0x25A0 && code <= 0x25FF) ||
      (code >= 0x2B00 && code <= 0x2BFF) ||
      code === 0x2122
    ) {
      return true;
    }
  }
  return false;
}

export function OAuthComplete() {
  const [params] = useSearchParams();
  const { restoreSession } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();

  const pendingToken = params.get("pending");
  const [accountType, setAccountType] = useState<"Visitor" | "Organizer">("Visitor");
  const [orgName, setOrgName] = useState("");
  const [orgReason, setOrgReason] = useState("");
  const [orgExperience, setOrgExperience] = useState("");
  const [profileImageFile, setProfileImageFile] = useState<File | null>(null);
  const [profileImagePreview, setProfileImagePreview] = useState<string | null>(null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);

  if (!pendingToken) {
    return (
      <div className="center-screen">
        <div className="card" style={{ textAlign: "center" }}>
          <h1 className="brand-sm">EventSphere</h1>
          <h2>Invalid registration link</h2>
          <p className="muted">
            This registration link is invalid or has expired. Please try signing
            in again.
          </p>
          <button className="btn btn-primary" onClick={() => navigate("/login")}>
            Go to Login
          </button>
        </div>
      </div>
    );
  }

  const disabled = submitting;

  const validate = (): boolean => {
    const next: Record<string, string> = {};
    if (accountType === "Organizer") {
      if (orgName.trim().length < 2)
        next.orgName = "Organization name must be at least 2 characters.";
      else if (containsEmoji(orgName))
        next.orgName =
          "Organization name contains invalid characters. Emoji are not allowed.";
      if (orgReason.trim().length < 10)
        next.orgReason = "Please provide a reason (at least 10 characters).";
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (disabled) return;
    if (!validate()) return;

    setSubmitting(true);
    try {        // Upload profile image if selected
      let profileImageUrl: string | undefined;
      if (profileImageFile) {
        try {
          profileImageUrl = await uploadProfileImage(profileImageFile);
        } catch {
          addToast("error", "Failed to upload profile image.");
          setSubmitting(false);
          return;
        }
      }

      await completeOAuthRegistration(
        pendingToken,
        accountType,
        accountType === "Organizer" ? orgName.trim() : undefined,
        accountType === "Organizer" ? orgReason.trim() : undefined,
        accountType === "Organizer" ? orgExperience.trim() || undefined : undefined,
        profileImageUrl,
      );
      // Restore session to pick up the new user state.
      await restoreSession();
      addToast(
        "success",
        accountType === "Organizer"
          ? "Account created. Your organizer application is pending admin review."
          : "Welcome to EventSphere!",
      );
      navigate("/dashboard", { replace: true });
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.errors) {
          const mapped: Record<string, string> = {};
          for (const [key, msgs] of Object.entries(err.errors))
            mapped[key] = msgs[0];
          setErrors(mapped);
          addToast("error", "Please fix the highlighted fields.");
        } else {
          addToast("error", err.message);
        }
      } else if (err instanceof NetworkError) {
        addToast("error", err.message);
      } else {
        addToast("error", "Unable to complete registration. Please try again.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="center-screen">
      <form className="card auth-form" onSubmit={onSubmit} noValidate>
        <h1 className="brand-sm">EventSphere</h1>
        <h2>Welcome to EventSphere</h2>
        <p className="muted" style={{ marginBottom: "1rem" }}>
          How do you want to use EventSphere?
        </p>

        <fieldset style={{ border: "none", padding: 0, margin: "0.5rem 0" }}>
          <div style={{ display: "flex", gap: "1rem", marginBottom: "0.5rem" }}>
            <label
              style={{
                flex: 1,
                display: "flex",
                alignItems: "flex-start",
                gap: "0.5rem",
                padding: "0.75rem",
                border: "1px solid var(--ink-violet)",
                borderRadius: "0px",
                cursor: disabled ? "not-allowed" : "pointer",
                background: accountType === "Visitor" ? "rgba(250,229,155,0.3)" : "transparent",
                opacity: disabled ? 0.6 : 1,
              }}
            >
              <input
                type="radio"
                name="accountType"
                value="Visitor"
                checked={accountType === "Visitor"}
                onChange={() => setAccountType("Visitor")}
                disabled={disabled}
                style={{ marginTop: "0.2rem" }}
              />
              <div>
                <div style={{ fontWeight: 600, fontSize: "0.9rem" }}>Visitor</div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  Discover and participate in events.
                </div>
              </div>
            </label>
            <label
              style={{
                flex: 1,
                display: "flex",
                alignItems: "flex-start",
                gap: "0.5rem",
                padding: "0.75rem",
                border: "1px solid var(--ink-violet)",
                borderRadius: "0px",
                cursor: disabled ? "not-allowed" : "pointer",
                background: accountType === "Organizer" ? "rgba(250,229,155,0.3)" : "transparent",
                opacity: disabled ? 0.6 : 1,
              }}
            >
              <input
                type="radio"
                name="accountType"
                value="Organizer"
                checked={accountType === "Organizer"}
                onChange={() => setAccountType("Organizer")}
                disabled={disabled}
                style={{ marginTop: "0.2rem" }}
              />
              <div>
                <div style={{ fontWeight: 600, fontSize: "0.9rem" }}>Organizer</div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  Create and manage events.
                  <br />
                  Requires Admin approval.
                </div>
              </div>
            </label>
          </div>
        </fieldset>

        {accountType === "Organizer" && (
          <>
            <label htmlFor="orgName">Organization Name</label>
            <input
              id="orgName"
              value={orgName}
              onChange={(e) => setOrgName(e.target.value)}
              disabled={disabled}
              placeholder="Your organization or team name"
            />
            {errors.orgName && <p className="field-error">{errors.orgName}</p>}

            <label htmlFor="orgReason">
              Why do you want to organize events?
            </label>
            <textarea
              id="orgReason"
              value={orgReason}
              onChange={(e) => setOrgReason(e.target.value)}
              disabled={disabled}
              rows={3}
              placeholder="Tell us why you want to organize events on EventSphere"
              style={{ resize: "vertical" }}
            />
            {errors.orgReason && (
              <p className="field-error">{errors.orgReason}</p>
            )}

            <label htmlFor="orgExperience">Previous Experience (optional)</label>
            <textarea
              id="orgExperience"
              value={orgExperience}
              onChange={(e) => setOrgExperience(e.target.value)}
              disabled={disabled}
              rows={3}
              placeholder="Describe any previous event organizing experience"
              style={{ resize: "vertical" }}
            />
          </>
        )}

        <div style={{ textAlign: "center", margin: "0.75rem 0" }}>
          <div
            onClick={() => !disabled && document.getElementById("oauth-profile-image")?.click()}
            style={{
              display: "inline-block",
              width: 80,
              height: 80,
              borderRadius: "50%",
              cursor: disabled ? "not-allowed" : "pointer",
              overflow: "hidden",
              position: "relative",
              border: "2px dashed var(--ink-violet)",
            }}
          >
            {profileImagePreview ? (
              <img src={profileImagePreview} alt="Profile" style={{ width: "100%", height: "100%", objectFit: "cover" }} />
            ) : (
              <div style={{
                width: "100%",
                height: "100%",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: "0.75rem",
                color: "var(--muted)",
              }}>
                Add Photo
              </div>
            )}
          </div>
          <input
            id="oauth-profile-image"
            type="file"
            accept="image/*"
            style={{ display: "none" }}
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (!file) return;
              if (file.size > 2 * 1024 * 1024) {
                addToast("error", "Image must be less than 2MB.");
                return;
              }
              setProfileImageFile(file);
              const reader = new FileReader();
              reader.onload = () => setProfileImagePreview(reader.result as string);
              reader.readAsDataURL(file);
            }}
          />
          {profileImageFile && (
            <button
              type="button"
              className="btn btn-secondary"
              style={{ marginTop: "0.25rem", fontSize: "0.75rem", padding: "0.25rem 0.5rem" }}
              onClick={() => { setProfileImageFile(null); setProfileImagePreview(null); }}
              disabled={disabled}
            >
              Remove
            </button>
          )}
        </div>

        <button className="btn btn-primary" type="submit" disabled={disabled}>
          {submitting ? "Creating account…" : "Complete Registration"}
        </button>
      </form>
    </div>
  );
}
