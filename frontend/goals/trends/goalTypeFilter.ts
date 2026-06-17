import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
import type {
  GoalTrendsGoalType,
  GoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";

const assignmentGoalTypeValues = Object.values(
  AssignmentGoalType,
) as readonly AssignmentGoalType[];
const spendingGoalTypeValues = Object.values(
  SpendingGoalType,
) as readonly SpendingGoalType[];

/**
 * Gets the goal type values.
 */
function getGoalTypeValues(view: "assignment"): readonly AssignmentGoalType[];
function getGoalTypeValues(view: "spending"): readonly SpendingGoalType[];
function getGoalTypeValues(view: GoalTrendsView): readonly GoalTrendsGoalType[];
function getGoalTypeValues(
  view: GoalTrendsView,
): readonly GoalTrendsGoalType[] {
  return view === "assignment"
    ? assignmentGoalTypeValues
    : spendingGoalTypeValues;
}

/**
 * Normalizes raw goal-type values from the URL into a canonical ordered list.
 */
function normalizeGoalTypes(
  values: readonly string[],
  view: "assignment",
): readonly AssignmentGoalType[];
function normalizeGoalTypes(
  values: readonly string[],
  view: "spending",
): readonly SpendingGoalType[];
function normalizeGoalTypes(
  values: readonly string[],
  view: GoalTrendsView,
): readonly GoalTrendsGoalType[];
function normalizeGoalTypes(
  values: readonly string[],
  view: GoalTrendsView,
): readonly GoalTrendsGoalType[] {
  const goalTypeValues = getGoalTypeValues(view);
  const requestedGoalTypes = new Set<string>();

  values.forEach((value) => {
    const nextValue = value.trim();
    if (nextValue !== "") {
      requestedGoalTypes.add(nextValue);
    }
  });

  if (requestedGoalTypes.size === 0) {
    return [];
  }

  return goalTypeValues.filter((goalType) => requestedGoalTypes.has(goalType));
}

/**
 * Determines whether selected goal types should be written into the URL.
 */
const shouldPersistGoalTypes = function (
  values: readonly GoalTrendsGoalType[],
  view: GoalTrendsView,
): boolean {
  return values.length > 0 && values.length < getGoalTypeValues(view).length;
};

export { getGoalTypeValues, normalizeGoalTypes, shouldPersistGoalTypes };
