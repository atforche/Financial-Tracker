<!-- BEGIN:nextjs-agent-rules -->

## This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` (resolved from this file's directory; in monorepos the `next` package may not be visible from the repo root) before writing any code. Heed deprecation notices.

This block is written and re-added by `next dev` — verify at `node_modules/next/dist/server/lib/generate-agent-files.js`. Removing it from a diff only re-creates the uncommitted change; committing it with your work keeps the tree clean.

<!-- END:nextjs-agent-rules -->

## Frontend conventions

- Keep view state that should survive navigation in the URL when it is worth sharing or reloading (filters, tabs, date
  ranges), and in `usePageState` (`framework/routes/usePageState.ts`) otherwise. Expandable or collapsible UI, show-all
  toggles, and in-page search boxes use `usePageState` instead of `useState`, so returning to a page by a back link,
  Cancel, or a redirect after saving restores it as the user left it. Give each value a key that is unique within its
  page. Dialogs, menus, and form fields keep plain `useState`.
