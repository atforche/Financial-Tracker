I want to make a complete overhaul to how the application handles validation errors. Right now, the exception classes inherit from the base Exception class and there's mapping that needs to be setup correctly in the REST and Frontend layers in order to get exceptions propagated correctly. I want to improve this experience.

1. Stop using derived exception classes to denote exceptions. Instead create a new object hierarchy to represent validation errors collected and returned by the application.

1. On the new validation error objects, allow for the place that returns the validation error to fully specify the specific property that is the culprit for the validation error. The goal is that the error returned by the application should fully specify the property within the request that caused the error. This allows the consumer of the response to understand exactly what went wrong and where.

1. Redesign how the UI displays errors to take advantage of the new validation design and make it simpler to pass exception messages down to individual controls.