import { GoalType } from "@/goals/types";

const goalTypeValues = Object.values(GoalType) as readonly GoalType[];
const goalTypeSet = new Set<string>(goalTypeValues);

/**
 * Normalizes raw goal-type values from the URL into a canonical ordered list.
 */
const normalizeGoalTypes = function (
  values: readonly string[],
): readonly GoalType[] {
  const seenGoalTypes = new Set<string>();
  const normalizedGoalTypes: GoalType[] = [];

  values.forEach((value) => {
    const nextValue = value.trim();
    if (nextValue === "" || seenGoalTypes.has(nextValue)) {
      return;
    }
    if (!goalTypeSet.has(nextValue)) {
      return;
    }
    seenGoalTypes.add(nextValue);
    // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
    normalizedGoalTypes.push(nextValue as GoalType);
  });

  if (normalizedGoalTypes.length === 0) {
    return [];
  }

  return goalTypeValues.filter((goalType) =>
    normalizedGoalTypes.includes(goalType),
  );
};

/**
 * Determines whether selected goal types should be written into the URL.
 */
const shouldPersistGoalTypes = function (values: readonly GoalType[]): boolean {
  return values.length > 0 && values.length < goalTypeValues.length;
};

export { goalTypeValues, normalizeGoalTypes, shouldPersistGoalTypes };
