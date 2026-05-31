---
name: cashregister-ui
description: Use for Cash Register frontend work in React, React Router, TypeScript, Vite, Tailwind CSS, and Vitest.
---

# Cash Register UI Skill

## Owned Paths

UI work normally lives under:

- `ui/app/`
- `ui/public/`
- UI configuration and package files only when explicitly assigned.

Frontend runtime architecture is documented in `docs/ARCH.md`. Frontend implementation practice is documented in this skill.

## Conventions

- Use React 19 and React Router v7 framework mode with `ssr: false`.
- Register routes in `ui/app/routes.ts`.
- Keep each route in its own folder under `ui/app/routes/`.
- Use a route component file name that matches the route folder name.
- Keep route-local components in that route's `components/` folder.
- Keep shared components in `ui/app/components/`.
- Use one React component per `.tsx` file, except route files may also export loaders, actions, and boundaries.
- Use React Router generated `Route.ComponentProps` typing for `loaderData`.
- Use `<>...</>` fragments, not `<Fragment>`, unless an explicit keyed fragment is required.
- Use `@cashregister/*` imports for app modules except established local sibling route imports.
- Use `deps.apiClient` from `ui/app/deps.ts` for backend calls.

## API and Money Behavior

- Keep DTO interfaces in `ui/app/model.ts`.
- Preserve frontend `Result<T>` handling and explicit failure paths.
- Handle loader and action failures explicitly and surface user-facing failures through the existing error-message system.
- Keep money formatting and parsing in `ui/app/money.ts`.
- Use `ui/app/components/money-input.tsx` for user-entered money values.
- Users see decimal money strings; forms submit integer cent fields.

## Styling

- Use Tailwind CSS v4 through the Vite plugin and `ui/app/app.css`.
- Put shared component utility classes in `@layer components` in `ui/app/app.css`.
- Prefer shared component classes such as `btn-primary`, `btn-secondary`, `btn-outline`, and `input-field`.
- Keep kiosk layouts stable and avoid hover or state changes that resize tables, controls, or summary panels.

## Testing

- Keep frontend tests colocated under `ui/app/**/*.test.{ts,tsx}`.
- Use Vitest, jsdom, React Testing Library, and `@testing-library/user-event`.
- Mock `deps.apiClient` for route loader/action tests when backend behavior is not under test.
- Prefer assertions on user-visible behavior.

## Validation

For UI source changes, run from `ui/`:

    npm run lint
    npm run build
    npm run test

For documentation-only UI coordination changes, run `git diff --check` from the repository root.

## Common Pitfalls

- Do not create ad hoc fetch wrappers.
- Do not duplicate money conversion logic.
- Do not invent API contracts in UI code.
- Do not add package dependencies without explicit assignment.
