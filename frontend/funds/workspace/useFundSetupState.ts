"use client";

import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
import { type Dispatch, type SetStateAction, useState } from "react";

/**
 * State for the fund setup section of the create and onboarding fund forms.
 */
interface FundSetupState {
  readonly name: string;
  readonly setName: Dispatch<SetStateAction<string>>;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly assignmentGoalType: AssignmentGoalType | null;
  readonly setAssignmentGoalType: Dispatch<
    SetStateAction<AssignmentGoalType | null>
  >;
  readonly assignmentGoalAmount: number | null;
  readonly setAssignmentGoalAmount: Dispatch<SetStateAction<number | null>>;
  readonly spendingGoalType: SpendingGoalType | null;
  readonly setSpendingGoalType: Dispatch<
    SetStateAction<SpendingGoalType | null>
  >;
  readonly reset: () => void;
}

/**
 * Owns the fields shared by the create and onboarding fund forms.
 */
const useFundSetupState = function (): FundSetupState {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [assignmentGoalType, setAssignmentGoalType] =
    useState<AssignmentGoalType | null>(AssignmentGoalType.MonthlyTarget);
  const [assignmentGoalAmount, setAssignmentGoalAmount] = useState<
    number | null
  >(null);
  const [spendingGoalType, setSpendingGoalType] =
    useState<SpendingGoalType | null>(SpendingGoalType.Standard);

  const reset = function (): void {
    setName("");
    setDescription("");
    setAssignmentGoalType(AssignmentGoalType.MonthlyTarget);
    setAssignmentGoalAmount(null);
    setSpendingGoalType(SpendingGoalType.Standard);
  };

  return {
    name,
    setName,
    description,
    setDescription,
    assignmentGoalType,
    setAssignmentGoalType,
    assignmentGoalAmount,
    setAssignmentGoalAmount,
    spendingGoalType,
    setSpendingGoalType,
    reset,
  };
};

export type { FundSetupState };
export default useFundSetupState;
