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
`GOOGLE_CLIENT_SECRET`, and `GOOGLE_ALLOWED_SUBJECTS`. Never place those values in
the tracked template; use a local ignored environment file or an approved secret
store instead.

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
