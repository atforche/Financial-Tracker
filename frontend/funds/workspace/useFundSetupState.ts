"use client";

import { type Dispatch, type SetStateAction, useState } from "react";

/**
 * State for the fund setup section of the create and onboarding fund forms.
 */
interface FundSetupState {
  readonly name: string;
  readonly setName: Dispatch<SetStateAction<string>>;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly plannedMonthlyContribution: number | null;
  readonly setPlannedMonthlyContribution: Dispatch<
    SetStateAction<number | null>
  >;
  readonly minimumEndingBalance: number | null;
  readonly setMinimumEndingBalance: Dispatch<SetStateAction<number | null>>;
  readonly maximumEndingBalance: number | null;
  readonly setMaximumEndingBalance: Dispatch<SetStateAction<number | null>>;
  readonly reset: () => void;
}

/**
 * Owns the fields shared by the create and onboarding fund forms.
 */
const useFundSetupState = function (): FundSetupState {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [plannedMonthlyContribution, setPlannedMonthlyContribution] = useState<
    number | null
  >(null);
  const [minimumEndingBalance, setMinimumEndingBalance] = useState<
    number | null
  >(null);
  const [maximumEndingBalance, setMaximumEndingBalance] = useState<
    number | null
  >(null);
  const reset = (): void => {
    setName("");
    setDescription("");
    setPlannedMonthlyContribution(null);
    setMinimumEndingBalance(null);
    setMaximumEndingBalance(null);
  };
  return {
    name,
    setName,
    description,
    setDescription,
    plannedMonthlyContribution,
    setPlannedMonthlyContribution,
    minimumEndingBalance,
    setMinimumEndingBalance,
    maximumEndingBalance,
    setMaximumEndingBalance,
    reset,
  };
};

export type { FundSetupState };
export default useFundSetupState;
