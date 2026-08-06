# Financial-Tracker

Financial tracking application to track different aspects of financial well-being

## Local development

The normal local workflow does not require a Google account or OAuth credentials.
It uses a fixed local developer identity that is accepted only by an API running
with `ASPNETCORE_ENVIRONMENT=Development`.

Create the ignored local configuration, SQLite database, and schema once for
native VS Code debugging:

```sh
./scripts/debug_scripts.py create
```

Then use one of the VS Code launch configurations.

Local development provides three selectable identities on the login page:
Administrator, Standard user, and Read-only user. The migrator seeds these
database users when `AUTH_MODE=development`, so their permissions match the
production authorization rules.

### Full local container stack

Start the complete local stack from source with:

```sh
./scripts/debug_scripts.py stack-up
```

The application is available at `http://localhost:3001`; the API is at
`http://localhost:8081`. The command creates the disposable `debug/.env`
configuration from `scripts/debug.env.example` if it is absent, builds the
images, applies migrations, and waits for the services to be healthy.

Stop the services while retaining the local SQLite data:

```sh
./scripts/debug_scripts.py stack-down
```

Remove the services and their local Compose volumes:

```sh
./scripts/debug_scripts.py stack-destroy
```

For Docker troubleshooting, the underlying command remains available:

```sh
docker compose -f compose.dev.yaml up --build --detach --wait
```

`AUTH_MODE=google` remains the default and is intended for production or explicit
Google integration testing. It requires `GOOGLE_CLIENT_ID`,
and `GOOGLE_CLIENT_SECRET`. Application access is determined by the database-backed
user and invitation flow, not an environment allowlist. Never place credentials in
the tracked template; use a local ignored environment file or an approved secret store.

## Production deployment

Install Docker and a self-hosted GitHub Actions runner with the `production` label on
the target host. Configure these values in the GitHub `production` environment:

- Variables: `INSTANCE_PATH`, `INSTANCE_NAME`, `ENVIRONMENT` (normally `Production`),
  `PUBLIC_ORIGIN`, `GOOGLE_CLIENT_ID`, and `INITIAL_ADMIN_EMAIL`.
- Secrets: `GOOGLE_CLIENT_SECRET` and `AUTH_SECRET`.

Dispatch **Deploy Production** from `main` with the full commit SHA and successful
Release workflow run ID. It creates an absent `INSTANCE_PATH` as a new instance;
otherwise it transactionally deploys to the existing instance and retains the prior
release for rollback. A path that exists but is not a valid instance fails rather than
being overwritten.

### User-management cutover

Before deploying the user-management migration, run the **Backup Production**
workflow and verify its latest restore. Deployment automatically creates the initial
administrator invitation for `INITIAL_ADMIN_EMAIL` when no active administrator exists.
It is safe to repeat for that email, including while its invitation is pending. The
deployment archives the pre-migration SQLite database locally and retains the prior
release for rollback, but a rollback does not make a migrated database compatible with
an older application image.

Verify that administrator's Google login before inviting collaborators. Do not import
former allowlisted subjects: each collaborator must be invited by email and accept
through a verified Google login.
