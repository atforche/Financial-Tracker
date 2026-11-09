/**
 * Determines whether an account goal's bounds are in order. A goal without a maximum always is.
 */
const isAccountGoalRangeValid = function (
  minimumEndingBalance: number,
  maximumEndingBalance: number | null,
): boolean {
  return (
    maximumEndingBalance === null ||
    minimumEndingBalance <= maximumEndingBalance
  );
};

export default isAccountGoalRangeValid;
