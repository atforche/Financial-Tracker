# Financial-Tracker
Financial tracking application to track different aspects of financial well-being

## Production bootstrap

The first deployment to a new host is performed through the **Bootstrap Production**
workflow. It creates the configured `INSTANCE_PATH`, initializes its SQLite database,
applies migrations, and starts the release. It refuses to run if that path already
exists, so it cannot overwrite an existing environment.

Before dispatching the workflow, install Docker and a self-hosted GitHub Actions runner
with the `production` label on the target host. Configure these values in the GitHub
`production` environment:

- Variables: `INSTANCE_PATH`, `INSTANCE_NAME`, `ENVIRONMENT` (normally `Production`),
  `PUBLIC_ORIGIN`, `GOOGLE_CLIENT_ID`, and `GOOGLE_ALLOWED_SUBJECTS`.
- Secrets: `GOOGLE_CLIENT_SECRET` and `AUTH_SECRET`.

Dispatch the workflow from `main` with the full commit SHA and successful Release
workflow run ID. All later releases use **Deploy Production**, which retains its
transactional rollback behavior.
