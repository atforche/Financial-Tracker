# Production Deployment Readiness

This application is not ready to be publicly deployed with financial data yet. Complete the P0 items before public launch.

## P0 — Block public launch

1. Add authentication and per-user authorization before exposing the app.

   Every controller is reachable without authentication, including mutations and deletes; `UseAuthorization` is configured without authentication or authorization policies. A public backend port means anyone who reaches it can read and modify financial data. See `backend/Rest/Program.cs:88` and `backend/Rest/Transactions/TransactionController.cs:120`.

2. Put the app behind a TLS reverse proxy and expose only that proxy/frontend publicly.

   Compose publishes both frontend and backend ports directly, while the backend's HTTPS middleware does not provide TLS termination. Use Caddy, nginx, or a managed load balancer for HTTPS, security headers, forwarded headers, and request limits; keep the API on the internal Compose network. See `compose.yaml:7`.

3. Replace the stop-first deployment with a verified release and rollback procedure.

   `deploy` brings the instance down, runs database changes, and builds images—but does not restart it. A migration or build failure leaves the service offline, with no rollback or post-deploy smoke check. Build and validate immutable images first, snapshot the database, migrate, start, wait for readiness, then retain the previous image/database recovery point. See `scripts/deploy_scripts.py:93`.

4. Implement automated, encrypted backups and prove restoration.

   The only durable production data is a host-mounted SQLite file; migration archives are created only during migrations, not as recurring backups. Schedule off-host encrypted backups, retain several recovery points, and regularly restore into a clean environment and verify balances. See `compose.yaml:9`.

## P1 — Complete before relying on the deployment

5. Add real readiness/liveness endpoints and operational telemetry.

   The database check runs only during startup; no health endpoint exists for a proxy or monitor. Add `/health/live` and `/health/ready`, structured request logs with correlation IDs, error alerting, uptime checks, and bounded log retention. Current logs roll daily but have no retention policy. See `backend/Data/DatabaseContext.cs:83` and `backend/Rest/Program.cs:40`.

6. Harden containers and Compose runtime policy.

   The backend image runs as root, base images use mutable tags, and Compose has no restart policy, health checks, resource limits, or container security restrictions. Pin image digests, run as an unprivileged user, use read-only filesystems where practical, cap CPU/memory, and configure restart/health behavior. See `backend/Dockerfile:1` and `compose.yaml:3`.

7. Introduce secret management and stop logging all environment values.

   There are no application secrets today, but authentication, backup encryption, and future integrations will need them. Use the hosting platform's secret store, add `.env` protection plus a sanitized `.env.example`, and whitelist—not enumerate—values safe to log. The current environment logger prints every public property. See `backend/Data/EnvironmentVariableManager.cs:14` and `.gitignore:1`.

8. Extend CI into a release-security pipeline.

   CI runs only for pull requests and does not build/publish deployable images, scan dependencies/images, create an SBOM, sign artifacts, or run a container-level smoke test. Add protected main/release workflows, dependency/image scanning, pinned action versions, and deployment promotion by immutable image digest. See `.github/workflows/pipeline.yml:3`.

## P2 — Plan for the intended operating model

9. Decide whether SQLite matches the intended operating model.

   SQLite can be excellent for a single-host, low-write personal deployment, but it constrains horizontal scaling, failover, and concurrent writes. Document that boundary; if multi-user/high-availability is intended, plan a managed PostgreSQL migration before users depend on it. See `backend/Data/DatabaseContext.cs:108`.

10. Write and rehearse a minimal operations runbook.

    The README currently contains no deploy, restore, incident, upgrade, certificate-renewal, or monitoring instructions. Document the deployment contract, backups, rollback decision points, alert ownership, and a quarterly restore/disaster-recovery exercise. See `README.md:1`.

## Recommended implementation order

Complete P0 access control, TLS, safe releases, and backup/recovery first. Then add monitoring, container hardening, secret management, and release-pipeline controls. Only then consider public launch.
