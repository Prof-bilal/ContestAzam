# razor/SKILL.md — Razor Syntax and Patterns

## Purpose

Guide agents to correctly use Razor syntax, Tag Helpers, ViewModels, partials, and layouts.

## When To Use

- Creating or modifying `.cshtml` files.
- Working with ViewModels in views.
- Using Tag Helpers.
- Creating partials or layout sections.

## Razor Syntax Reference

```razor
@model EventSphere.Web.ViewModels.Events.EventDetailViewModel

<!-- C# expressions -->
<p>@Model.Event.Title</p>

<!-- Loops -->
@foreach (var item in Model.Items) {
    <div>@item.Name</div>
}

<!-- Conditionals -->
@if (User.Identity?.IsAuthenticated == true) {
    <p>Welcome!</p>
}

<!-- Tag Helpers -->
<a asp-controller="Events" asp-action="Details" asp-route-id="@item.Id">View</a>
<form asp-controller="Events" asp-action="Register" method="post">
    <input type="hidden" name="eventId" value="@Model.Event.Id" />
    <button type="submit">Register</button>
</form>

<!-- Partial -->
<partial name="Partials/_EventCard" model="evt" />

<!-- Section -->
@section Scripts {
    <script src="~/js/custom.js"></script>
}
```

## Rules

- Always use `@model` directive.
- Use Tag Helpers for URLs — never hardcode.
- Never put business logic in `.cshtml`.
- Never use `@Html.Raw()` with untrusted input.
- One `@model` per view.
