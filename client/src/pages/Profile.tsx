import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import {
  getProfile,
  updateProfile,
  deleteAccount,
  submitOrganizerRequest,
  ApiError,
} from "../api/client";
import type { ProfileDto } from "../types";

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

export function Profile() {
  const { logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();

  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState("");
  const [editMobile, setEditMobile] = useState("");
  const [editDepartment, setEditDepartment] = useState("");
  const [editImage, setEditImage] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Delete account
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deleting, setDeleting] = useState(false);

  // Organizer application
  const [showOrgForm, setShowOrgForm] = useState(false);
  const [orgName, setOrgName] = useState("");
  const [orgReason, setOrgReason] = useState("");
  const [orgExperience, setOrgExperience] = useState("");
  const [submittingOrg, setSubmittingOrg] = useState(false);
  const [orgErrors, setOrgErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    void loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      const p = await getProfile();
      setProfile(p);
      setEditName(p.fullName ?? p.name);
      setEditMobile(p.mobile ?? "");
      setEditDepartment(p.department ?? "");
      setEditImage(p.profileImageUrl ?? null);
    } catch {
      addToast("error", "Unable to load profile.");
    } finally {
      setLoading(false);
    }
  };

  const startEdit = () => {
    if (!profile) return;
    setEditName(profile.fullName ?? profile.name);
    setEditMobile(profile.mobile ?? "");
    setEditDepartment(profile.department ?? "");
    setEditImage(profile.profileImageUrl ?? null);
    setErrors({});
    setEditing(true);
  };

  const cancelEdit = () => {
    setEditing(false);
    setErrors({});
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      addToast("error", "Image must be less than 2MB.");
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      setEditImage(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const saveProfile = async (e: FormEvent) => {
    e.preventDefault();
    if (saving) return;

    const next: Record<string, string> = {};
    if (editName.trim().length < 2) next.fullName = "Name must be at least 2 characters.";
    else if (containsEmoji(editName))
      next.fullName = "Name contains invalid characters. Emoji are not allowed.";
    setErrors(next);
    if (Object.keys(next).length > 0) return;

    setSaving(true);
    try {
      await updateProfile(
        editName.trim(),
        editMobile.trim() || undefined,
        editDepartment.trim() || undefined,
        editImage ?? undefined,
      );
      await loadProfile();
      setEditing(false);
      addToast("success", "Profile updated successfully.");
    } catch (err) {
      if (err instanceof ApiError && err.errors) {
        const mapped: Record<string, string> = {};
        for (const [key, msgs] of Object.entries(err.errors)) mapped[key] = msgs[0];
        setErrors(mapped);
      } else {
        addToast("error", "Unable to update profile.");
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (deleting) return;
    setDeleting(true);
    try {
      await deleteAccount();
      await logout();
      addToast("success", "Account deleted successfully.");
      navigate("/", { replace: true });
    } catch {
      addToast("error", "Account deletion failed.");
      setDeleting(false);
    }
  };

  const submitOrg = async (e: FormEvent) => {
    e.preventDefault();
    if (submittingOrg) return;

    const next: Record<string, string> = {};
    if (orgName.trim().length < 2)
      next.orgName = "Organization name must be at least 2 characters.";
    else if (containsEmoji(orgName))
      next.orgName =
        "Organization name contains invalid characters. Emoji are not allowed.";
    if (orgReason.trim().length < 10)
      next.orgReason = "Please provide a reason (at least 10 characters).";
    setOrgErrors(next);
    if (Object.keys(next).length > 0) return;

    setSubmittingOrg(true);
    try {
      await submitOrganizerRequest(
        orgName.trim(),
        orgReason.trim(),
        orgExperience.trim() || undefined,
      );
      await loadProfile();
      setShowOrgForm(false);
      addToast("success", "Organizer application submitted.");
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.errors) {
          const mapped: Record<string, string> = {};
          for (const [key, msgs] of Object.entries(err.errors))
            mapped[key] = msgs[0];
          setOrgErrors(mapped);
        } else {
          addToast("error", err.message);
        }
      } else {
        addToast("error", "Organizer application failed.");
      }
    } finally {
      setSubmittingOrg(false);
    }
  };

  if (loading) {
    return <div className="center-screen">Loading profile...</div>;
  }

  if (!profile) {
    return <div className="center-screen">Unable to load profile.</div>;
  }

  const isOrganizer = profile.roles.includes("Organizer");
  const isAdmin = profile.roles.includes("Admin");
  const hasPendingRequest = profile.organizerRequestStatus === "Pending";
  const hasRejectedRequest = profile.organizerRequestStatus === "Rejected";

  return (
    <div className="dashboard">
      <header className="dash-header">
        <h1 className="brand-sm">EventSphere</h1>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          {isAdmin && (
            <Link to="/admin" className="btn btn-secondary" style={{ textDecoration: "none" }}>
              Admin
            </Link>
          )}
          <Link to="/dashboard" className="btn btn-secondary" style={{ textDecoration: "none" }}>
            Dashboard
          </Link>
        </div>
      </header>

      {/* Profile Info */}
      <section className="card">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <h2>Profile</h2>
          {!editing && (
            <button className="btn btn-small" onClick={startEdit}>
              Edit Profile
            </button>
          )}
        </div>

        {editing ? (
          <form onSubmit={saveProfile} noValidate>
            {/* Profile image upload */}
            <div style={{ textAlign: "center", marginBottom: "1rem" }}>
              <div
                className="profile-avatar-edit"
                onClick={() => document.getElementById("profile-image-input")?.click()}
              >
                {editImage ? (
                  <img src={editImage} alt="Profile" className="profile-avatar-img" />
                ) : (
                  <div className="profile-avatar-placeholder">
                    {editName.trim().charAt(0).toUpperCase() || "?"}
                  </div>
                )}
                <div className="profile-avatar-overlay">Change Photo</div>
              </div>
              <input
                id="profile-image-input"
                type="file"
                accept="image/*"
                onChange={handleImageUpload}
                style={{ display: "none" }}
              />
              {editImage && (
                <button
                  type="button"
                  className="btn btn-small btn-secondary"
                  onClick={() => setEditImage(null)}
                  style={{ marginTop: "0.5rem" }}
                >
                  Remove Photo
                </button>
              )}
            </div>

            <label htmlFor="fullName">Full Name</label>
            <input
              id="fullName"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              disabled={saving}
            />
            {errors.fullName && <p className="field-error">{errors.fullName}</p>}

            <label htmlFor="mobile">Phone</label>
            <input
              id="mobile"
              value={editMobile}
              onChange={(e) => setEditMobile(e.target.value)}
              disabled={saving}
              placeholder="Optional"
            />

            <label htmlFor="department">Department</label>
            <input
              id="department"
              value={editDepartment}
              onChange={(e) => setEditDepartment(e.target.value)}
              disabled={saving}
              placeholder="Optional"
            />

            <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
              <button className="btn btn-primary" type="submit" disabled={saving} style={{ flex: 1 }}>
                {saving ? "Saving..." : "Save Changes"}
              </button>
              <button
                className="btn btn-secondary"
                type="button"
                onClick={cancelEdit}
                disabled={saving}
              >
                Cancel
              </button>
            </div>
          </form>
        ) : (
          <>
            {/* Profile image display */}
            <div style={{ textAlign: "center", marginBottom: "1rem" }}>
              {profile.profileImageUrl ? (
                <img
                  src={profile.profileImageUrl}
                  alt="Profile"
                  className="profile-avatar-img"
                />
              ) : (
                <div className="profile-avatar-placeholder profile-avatar-large">
                  {(profile.fullName || profile.name).charAt(0).toUpperCase()}
                </div>
              )}
            </div>
            <div className="profile-info">
              <div className="profile-field">
                <span className="profile-label">Name</span>
                <span>{profile.fullName || profile.name}</span>
              </div>
              <div className="profile-field">
                <span className="profile-label">Email</span>
                <span>{profile.email}</span>
              </div>
              <div className="profile-field">
                <span className="profile-label">Role</span>
                <span>
                  {(() => {
                    const rolePriority = ["Admin", "Organizer", "Participant", "Visitor"];
                    const highest = rolePriority.find((r) => profile.roles.includes(r));
                    return highest ? (
                      <span className="role-badge">{highest}</span>
                    ) : null;
                  })()}
                </span>
              </div>
              <div className="profile-field">
                <span className="profile-label">Email Verified</span>
                <span style={{ color: profile.emailConfirmed ? "var(--ok)" : "var(--danger)" }}>
                  {profile.emailConfirmed ? "Yes" : "No"}
                </span>
              </div>
              {profile.mobile && (
                <div className="profile-field">
                  <span className="profile-label">Phone</span>
                  <span>{profile.mobile}</span>
                </div>
              )}
              {profile.department && (
                <div className="profile-field">
                  <span className="profile-label">Department</span>
                  <span>{profile.department}</span>
                </div>
              )}
              {isOrganizer && profile.organizationName && (
                <div className="profile-field">
                  <span className="profile-label">Organization</span>
                  <span>{profile.organizationName}</span>
                </div>
              )}
              <div className="profile-field">
                <span className="profile-label">Member Since</span>
                <span>{new Date(profile.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
          </>
        )}
      </section>

      {/* Become Organizer */}
      {!isOrganizer && !isAdmin && (
        <section className="card">
          <h3>Organizer</h3>
          {hasPendingRequest && (
            <div className="status-banner status-pending">
              Organizer application is under review.
            </div>
          )}
          {hasRejectedRequest && (
            <div className="status-banner status-rejected">
              Your previous application was rejected. You may apply again.
            </div>
          )}
          {!hasPendingRequest && !showOrgForm && (
            <>
              <p className="muted">
                Want to create and manage events on EventSphere?
              </p>
              <button
                className="btn btn-primary"
                onClick={() => setShowOrgForm(true)}
              >
                Become an Organizer
              </button>
            </>
          )}
          {showOrgForm && (
            <form onSubmit={submitOrg} noValidate>
              <label htmlFor="orgName">Organization Name</label>
              <input
                id="orgName"
                value={orgName}
                onChange={(e) => setOrgName(e.target.value)}
                disabled={submittingOrg}
                placeholder="Your organization or team name"
              />
              {orgErrors.orgName && (
                <p className="field-error">{orgErrors.orgName}</p>
              )}

              <label htmlFor="orgReason">
                Why do you want to organize events?
              </label>
              <textarea
                id="orgReason"
                value={orgReason}
                onChange={(e) => setOrgReason(e.target.value)}
                disabled={submittingOrg}
                rows={3}
                placeholder="Tell us why you want to organize events"
                style={{ resize: "vertical" }}
              />
              {orgErrors.orgReason && (
                <p className="field-error">{orgErrors.orgReason}</p>
              )}

              <label htmlFor="orgExperience">Previous Experience (optional)</label>
              <textarea
                id="orgExperience"
                value={orgExperience}
                onChange={(e) => setOrgExperience(e.target.value)}
                disabled={submittingOrg}
                rows={3}
                placeholder="Any previous event organizing experience"
                style={{ resize: "vertical" }}
              />

              <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
                <button
                  className="btn btn-primary"
                  type="submit"
                  disabled={submittingOrg}
                  style={{ flex: 1 }}
                >
                  {submittingOrg ? "Submitting..." : "Submit Application"}
                </button>
                <button
                  className="btn btn-secondary"
                  type="button"
                  onClick={() => setShowOrgForm(false)}
                  disabled={submittingOrg}
                >
                  Cancel
                </button>
              </div>
            </form>
          )}
        </section>
      )}

      {/* Account Settings / Delete */}
      <section className="card">
        <h3>Account Settings</h3>
        {!showDeleteConfirm ? (
          <button
            className="btn btn-danger"
            onClick={() => setShowDeleteConfirm(true)}
          >
            Delete Account
          </button>
        ) : (
          <div className="delete-confirm">
            <p>
              Are you sure you want to delete your account? This action cannot be
              undone.
            </p>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button
                className="btn btn-primary"
                onClick={handleDelete}
                disabled={deleting}
                style={{
                  background: "var(--danger)",
                  flex: 1,
                }}
              >
                {deleting ? "Deleting..." : "Yes, Delete My Account"}
              </button>
              <button
                className="btn btn-secondary"
                onClick={() => setShowDeleteConfirm(false)}
                disabled={deleting}
              >
                Cancel
              </button>
            </div>
          </div>
        )}
      </section>
    </div>
  );
}
