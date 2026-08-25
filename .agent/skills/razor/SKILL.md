# razor/SKILL.md — Razor Syntax and Patterns

## Purpose

Guide agents to correctly use Razor syntax, Tag Helpers, ViewModels, partials, and layouts.

## When To Use

- Creating or modifying `.cshtml` files.
- Working with ViewModels in views.
- Using Tag Helpers.
- Creating partials or layout sections.

## Inputs

- The `.cshtml` file being modified.
- The ViewModel it uses.
- `_ViewImports.cshtml` for available Tag Helpers.

## Preconditions

- Understand Razor syntax (`@model`, `@Html`, `@section`).
- Read `_ViewImports.cshtml` for global imports.
- Read `_Layout.cshtml` for layout structure.

## Workflow

1. **Declare model**: `@model MyViewModel` at top of file.
2. **Use Tag Helpers**: `asp-controller`, `asp-action`, `asp-route-*`, `asp-for`.
3. **Use validation**: `asp-validation-for`, `asp-validation-summary`.
4. **Use partials**: `<partial name="Partials/_Name" model="item" />`.
5. **Use sections**: `@section Scripts { ... }` in view, `@RenderSectionAsync()` in layout.
6. **Avoid business logic**: Keep `@{ }` blocks minimal.

## Razor Syntax Reference

```razor
@model EventSphere.Web.ViewModels.Events.EventDetailViewModel

<!-- C# expressions -->
<p>@Model.Event.Title</p>
<p>@(Model.IsFree ? "Free" : "$" + Model.TicketPrice)</p>

<!-- Loops -->
@foreach (var item in Model.Items) {
    <div>@item.Name</div>
}

<!-- Conditionals -->
@if (User.Identity?.IsAuthenticated == true) {
    <p>Welcome!</p>
} else {
    <p>Please login.</p>
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
- Use Tag Helpers for URLs and forms — never hardcode URLs.
- Never put business logic in `.cshtml`.
- Never use `@Html.Raw()` with untrusted input.
- Use `ViewData`/`ViewBag` sparingly — prefer ViewModels.
- One `@model` per view.

## Verification

Build compiles without Razor errors. View renders correctly in browser.
