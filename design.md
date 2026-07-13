# Validation Error Redesign

## Purpose

Replace validation-specific exception classes with structured validation error
objects. The new design must let validation code identify the exact request
property responsible for an error, so REST can return standard ASP.NET
validation responses and the UI can attach messages to controls without
endpoint-specific error-mapping code.

## Initial scope

Implement this redesign for **accounts** first, covering all account validation
flows (create, onboard, update, delete, and related account operations as
applicable). The target architecture is intended to cover every validation flow
eventually, including accounting periods, funds, goals, and transactions.

Do not broaden the first implementation beyond accounts unless doing so is
necessary for a shared abstraction used by accounts.

## Decisions

- Validation failures are values, not exceptions. Remove custom validation
  exception classes such as `InvalidNameException` and `InvalidAmountException`.
- Keep normal exceptions for unexpected failures, programming errors, and other
  non-validation exceptional conditions.
- Continue returning ASP.NET Core `ValidationProblemDetails` for validation
  responses. In particular, retain its `errors` dictionary rather than inventing
  a new response envelope.
- The dictionary key is the error's final request-property path and its value is
  the associated message or messages.
- A true entity-level error has the empty path (`""`), which serializes as the
  existing form-level error key.
- A cross-field rule produces an error for **each** relevant property, rather
  than one ambiguous entity-level error.

## Validation error model

Introduce a small object hierarchy or equivalent value-object design for
validation errors. Its essential data is:

- a local property path (which may be empty), and
- a human-readable validation message.

The model must support immutable path composition. A lower-level validator
reports only the context it owns; a caller adds surrounding context as it
becomes known. For example:

1. Fund validation reports path `fund`.
2. The caller validating a collection recognizes the second item and prefixes
   or indexes the path to produce `fund[1]`.
3. A higher caller may prefix another owned property, producing a final path
   such as `destinations[1].fund`.

Composition must preserve the original error message and must not mutate the
original error. Define and use one consistent path syntax compatible with model
binding and the generated API client (for example, `items[0].property`).

Domain code should not need to know the complete HTTP request shape. It should
name the local domain property it can identify. Higher layers are responsible
for adding collection indexes and request-model context. The REST boundary is
responsible for returning only fully resolved request paths.

## Backend flow

The desired account validation flow is:

```text
domain validator returns ValidationError values with local paths
        -> service combines/prefixes errors as it adds context
        -> account REST controller converts final paths to
           ValidationProblemDetails.Errors
        -> HTTP 422 response
```

Controllers must no longer infer a field from an exception type or inspect an
error message's wording. This eliminates mappings such as `InvalidNameException
=> Name` and message-text checks.

Services and domain objects should return collections of validation errors in
their `Try...` validation APIs instead of `IEnumerable<Exception>`. Update
callers, tests, and helpers accordingly. When a service aggregates errors from
children, retain every error and compose its path at the point where the child
is located in the parent request.

For an account REST endpoint, build `ValidationProblemDetails.Errors` by
grouping final validation errors on their path and collecting their messages.
The response remains a 422 Unprocessable Entity response with the existing
validation-problem content type/shape.

Framework/model-binding validation is intentionally unchanged: it already
produces `ValidationProblemDetails`, which is why the application validation
responses retain that same contract.

## Frontend flow

The frontend should treat `ValidationProblemDetails.errors` as a generic map of
property paths to messages. Avoid endpoint-specific server-action state such as
`nameErrors`, `typeErrors`, `accountingPeriodErrors`, and `unmappedErrors` when
the same result can be represented as a path-keyed error map.

Forms should receive or access that map and look up errors by the path owned by
each control. A shared helper may format a property's list of messages for the
control. Form-level UI displays only errors under the empty path (and, during a
transition if necessary, truly unmapped errors); it must not be the normal home
for a field error.

Use the same path syntax on the frontend as the backend. This is particularly
important before transaction and other collection-heavy forms adopt the design.

## Migration plan

1. Define the reusable validation-error abstraction and path-composition API.
2. Convert account domain/service validation APIs and remove the account uses of
   custom validation exceptions.
3. Simplify account REST controllers to serialize error paths directly into
   `ValidationProblemDetails`.
4. Refactor account frontend actions and forms to pass the generic error map to
   their controls.
5. Add or update tests for local paths, prefix/index composition, grouped REST
   errors, empty-path errors, cross-field errors, and account form placement.
6. After the account implementation is stable, migrate the remaining domains
   incrementally using the same abstraction. Remove obsolete exception classes
   only when no remaining domain uses them.

## Success criteria

- Account validation errors identify the correct request property without
  controller-side type or message matching.
- Multiple messages for one property are retained.
- Entity-level errors retain the empty key.
- Cross-field rules appear beside every relevant control.
- The API contract remains `ValidationProblemDetails`.
- Account UI code no longer manually translates every API error key into a
  separate field-specific state property.
