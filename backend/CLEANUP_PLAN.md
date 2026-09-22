# Backend cleanup plan

## Purpose

Make the backend consistent to work in, reduce duplicated logic, and serve frontend pages with focused, efficient read contracts while preserving financial behavior. This plan covers `backend/` and the frontend changes needed to adopt new API contracts. [BASELINE.md](BASELINE.md) records the starting inventory and verification result.

The work is incremental. Complete the baseline and tests for a page before changing its reads, migrate that page, verify it, then remove contracts made obsolete by the migration. Do not make a single repository-wide rewrite.

## Outcomes

1. **One documented coding convention.** Add `backend/AGENTS.md` for architectural and code organization guidance. Keep mechanically enforceable formatting and naming rules in the existing `.editorconfig` files and `Directory.Build.props`; make the documents agree and run their checks in normal verification.
2. **Clear feature ownership.** Use a consistent path from REST endpoint through request/response mapping, application or query logic, domain rules, and data access. Give each feature an obvious home for its reads, mutations, models, and tests. Keep shared infrastructure in shared owners; allow a feature to omit layers it does not need.
3. **Less duplicate code.** Identify repeated calculations, validation, projections, pagination, and mapping. Consolidate code when the behavior and semantics truly match. Preserve domain-specific wrappers and names where similar code represents different financial concepts.
4. **Page-oriented reads.** Aim for one primary read request for each data-heavy frontend page or coherent page state. The endpoint returns a purpose-built model containing the data that page actually renders. Mutations, large independently paged collections, and sections with independent refresh needs may remain separate requests.
5. **Lean responses and efficient queries.** Select and calculate only the fields required by a consumer. Keep summary totals correct when lists are filtered or paginated. Avoid loading full entity graphs merely to construct a small response. Measure request count, transferred bytes, duration, and database query count using the same fixture before and after each migration.
6. **Coherent behavior tests.** Maintain a page-specific backend characterization test for each distinct data-bearing page read contract, plus focused domain tests for financial rules and lifecycle boundaries. Organize test classes by behavior with descriptive names. Do not lose edge cases while merging old classes.

## Boundaries and decisions

- A frontend route is not automatically a distinct backend contract. Redirects and forms that reuse a page's reads can share its characterization coverage. The page inventory must state which routes share a contract and which have unique reads.
- A page characterization test should exercise the application through its REST test host. A unit test remains appropriate for isolated calculations or domain rules. Assertions should cover the values the page uses, including financial totals, status, filtering, pagination, and empty states where relevant; a successful HTTP status alone is insufficient.
- Domain calculations remain authoritative in the backend. Page models can compose their results, but must not introduce a second formula for balances, goal progress, or accounting-period totals.
- Page models are read contracts. Mutation endpoints can retain resource-oriented shapes when that fits their actions. A new page read does not require merging unrelated writes into one endpoint.
- Keep authorization, validation, error status, and tenant or user scoping intact during contract migration. Generated frontend types must come from the current OpenAPI document.
- Remove an old route, model, converter, and test only after its frontend caller and any other legitimate consumer have migrated. Do not retain unused compatibility endpoints by default.

## Work sequence

### 1. Finish the baseline

- Complete [BASELINE.md](BASELINE.md) with a route-to-rendered-page map, including redirects, form data, client-side loaders, conditional requests, and alternative date/accounting-period modes.
- For representative seeded states, capture each page's actual request count, response bytes, duration, and backend query count. Record the fixture and measurement method so a later run is comparable. Distinguish the first render from filter, pagination, and detail interactions.
- Map each page to existing tests, identify missing behavior assertions, and add one page-specific characterization test for each distinct data-bearing page contract. Preserve the pre-change full-suite result as a reference, then run the full suite with the added tests.
- Record financial invariants and known edge cases that must remain true during restructuring: posted versus pending balances, period boundaries and copy-forward, contribution types, transfers, pagination-independent totals, and transaction ordering.

**Gate:** Every page chosen for migration has a current request trace, documented response fields in use, and a passing characterization test. No API redesign starts for that page before this gate.

### 2. Establish the style and structure rules

- Survey the current `Domain`, `Data`, `Models`, `Rest`, and `Tests` projects and their existing `.editorconfig` overrides. Choose one convention for namespaces, filenames, dependency direction, mapping, async/cancellation, nullable handling, and test naming.
- Write `backend/AGENTS.md` with examples and explicit ownership rules. Make only intentional `.editorconfig` or build-setting changes, and apply formatting separately from behavior changes so diffs remain reviewable.
- Define a representative feature layout, then use it as a template for later migrations. Keep feature-specific exceptions documented instead of adding empty layers for symmetry.

**Gate:** New backend work follows the documented convention; formatter, analyzer, build, and test checks pass.

### 3. Consolidate shared behavior

- Inventory near-duplicate code and identify the actual source of truth for each calculation or rule.
- Refactor one behavior at a time, with focused tests before moving callers. Prefer a small shared primitive when inputs and semantics match; retain separate feature APIs when they do not.
- Remove orphaned helpers and models after callers move. Do not combine code solely because its shape looks alike.

**Gate:** Existing and new behavior tests pass; duplicate implementations of a shared rule no longer disagree.

### 4. Redesign reads page by page

For each page, in an order based on measured cost and dependency risk:

1. Document the fields and interactions the page actually uses, its baseline trace, and the expected response shape. Include summary values separately from paginated row data.
2. Add or strengthen its characterization test, including at least one non-empty fixture and relevant filter or lifecycle edge cases.
3. Implement the focused query and page response, preserving backend financial semantics and authorization. Check query shape and payload size against the baseline fixture.
4. Regenerate the OpenAPI frontend client, move the page to the new contract, and verify its loading, filters, navigation, and mutation return paths.
5. Remove superseded read routes and types after auditing all frontend callers, generated references, tests, and internal consumers.
6. Record before/after request count, bytes, duration, and query count in the baseline document, including any tradeoff such as a larger single response or slower independent pagination.

**Gate:** The page's characterization and UI flows pass, response fields are all used or justified, and the measured result improves the intended cost without a material regression elsewhere.

### 5. Reorganize the test suite

- Group endpoint tests by page or user journey and domain tests by invariant or lifecycle. Use class and test names that identify the behavior under test.
- Merge overlapping setup and assertions while retaining distinct edge cases. Move generic fixtures into test infrastructure only when several features truly share them.
- Audit coverage for authorization, validation, empty data, boundaries, filters, sorting, pagination, and mutations. A test count alone is not an acceptance measure.

**Gate:** The full suite passes; the page-to-test map has no unexplained gaps; production-contract checks remain intact.

### 6. Final audit

- Recheck every frontend route and action against the surviving API. Verify generated types, raw requests, and OpenAPI operations have no stale contracts.
- Run build, formatter/analyzers, backend tests and coverage, frontend typecheck/lint, and targeted browser checks for migrated pages.
- Summarize the final API surface, page request and payload changes, query costs, test organization, and any remaining exceptions in the baseline document.

## Change discipline

Keep style-only changes, shared-code refactors, and API behavior changes in separate reviewable changes when practical. Preserve unrelated working-tree changes. When a page migration reveals an unexpected behavior difference, stop that migration, capture the difference in a test or trace, and resolve whether it is an existing contract or an explicitly approved behavior change before continuing.
