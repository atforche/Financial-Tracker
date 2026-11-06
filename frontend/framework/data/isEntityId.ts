const entityIdPattern =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/iu;
const emptyEntityId = "00000000-0000-0000-0000-000000000000";

/**
 * Determines whether a route parameter could identify an entity, since the API rejects anything else as a bad request.
 */
const isEntityId = function (value: string): boolean {
  return entityIdPattern.test(value) && value !== emptyEntityId;
};

export default isEntityId;
