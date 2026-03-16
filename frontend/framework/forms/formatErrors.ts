/**
 * Formats the provided list of errors to be displayed in the UI. If there are no errors, returns null.
 */
const formatErrors = function (errors: string[] | null): string | null {
  if (!errors || errors.length === 0) {
    return null;
  }
  return errors.join("\n");
};

export default formatErrors;
