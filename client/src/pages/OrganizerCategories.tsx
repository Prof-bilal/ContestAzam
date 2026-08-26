import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import {
  getOrganizerCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from "../api/client";
import type { EventCategory } from "../types";

export function OrganizerCategories() {
  const { addToast } = useToast();
  const [categories, setCategories] = useState<EventCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [saving, setSaving] = useState(false);
  const [deleteModal, setDeleteModal] = useState<EventCategory | null>(null);

  const fetchCategories = () => {
    setLoading(true);
    getOrganizerCategories()
      .then(setCategories)
      .catch(() => setCategories([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchCategories(); }, []);

  const resetForm = () => {
    setName("");
    setDescription("");
    setEditId(null);
    setShowForm(false);
  };

  const openCreate = () => {
    resetForm();
    setShowForm(true);
  };

  const openEdit = (cat: EventCategory) => {
    setEditId(cat.id);
    setName(cat.name);
    setDescription(cat.description ?? "");
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    setSaving(true);
    try {
      if (editId) {
        await updateCategory(editId, { name: name.trim(), description: description.trim() || undefined });
        addToast("success", "Category updated.");
      } else {
        await createCategory({ name: name.trim(), description: description.trim() || undefined });
        addToast("success", "Category created.");
      }
      resetForm();
      fetchCategories();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Failed to save category.";
      addToast("error", msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteModal) return;
    try {
      await deleteCategory(deleteModal.id);
      addToast("success", "Category deleted.");
      setDeleteModal(null);
      fetchCategories();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Failed to delete category.";
      addToast("error", msg);
    }
  };

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <nav className="admin-nav">
          <Link to="/organizer/events" className="admin-nav-item">Dashboard</Link>
          <Link to="/organizer/events/create" className="admin-nav-item">Create Event</Link>
          <Link to="/organizer/categories" className="admin-nav-item active">Categories</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
        </nav>
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1>Manage Categories</h1>
          <button className="btn btn-small" onClick={openCreate}>+ Add Category</button>
        </div>

        {showForm && (
          <div className="card" style={{ marginBottom: "1.25rem" }}>
            <h3 style={{ margin: "0 0 0.75rem" }}>{editId ? "Edit Category" : "New Category"}</h3>
            <form onSubmit={handleSubmit}>
              <label>Category Name *</label>
              <input
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                maxLength={50}
                placeholder="e.g. Technical, Cultural, Sports"
              />
              <label>Description</label>
              <input
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                maxLength={200}
                placeholder="Optional short description"
              />
              <div style={{ display: "flex", gap: "0.5rem", marginTop: "0.75rem" }}>
                <button className="btn btn-small" type="submit" disabled={saving || !name.trim()}>
                  {saving ? "Saving..." : editId ? "Update" : "Create"}
                </button>
                <button className="btn btn-secondary btn-small" type="button" onClick={resetForm}>
                  Cancel
                </button>
              </div>
            </form>
          </div>
        )}

        {loading ? (
          <div className="loading-state">Loading...</div>
        ) : categories.length === 0 ? (
          <div className="empty-state">
            <p>No categories yet. Create your first category to get started.</p>
          </div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Events</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((cat) => (
                  <tr key={cat.id}>
                    <td style={{ fontWeight: 600 }}>{cat.name}</td>
                    <td className="muted">{cat.description || "—"}</td>
                    <td>{cat.eventCount}</td>
                    <td>
                      <div style={{ display: "flex", gap: "0.35rem" }}>
                        <button className="btn btn-secondary btn-small" onClick={() => openEdit(cat)}>
                          Edit
                        </button>
                        <button
                          className="btn btn-danger btn-small"
                          onClick={() => setDeleteModal(cat)}
                          disabled={cat.eventCount > 0}
                          title={cat.eventCount > 0 ? "Cannot delete category with events" : "Delete category"}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>

      {deleteModal && (
        <div className="modal-overlay" onClick={() => setDeleteModal(null)}>
          <div className="modal card" onClick={(e) => e.stopPropagation()}>
            <h3>Delete Category</h3>
            <p>
              Are you sure you want to delete <strong>{deleteModal.name}</strong>?
              {deleteModal.eventCount > 0 && (
                <span style={{ color: "var(--error)", display: "block", marginTop: "0.5rem" }}>
                  This category has {deleteModal.eventCount} event(s) and cannot be deleted.
                </span>
              )}
            </p>
            <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
              <button
                className="btn btn-danger btn-small"
                onClick={handleDelete}
                disabled={deleteModal.eventCount > 0}
              >
                Delete
              </button>
              <button className="btn btn-secondary btn-small" onClick={() => setDeleteModal(null)}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
