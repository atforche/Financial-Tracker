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
  readonly minimumEndingBalance: number;
  readonly setMinimumEndingBalance: Dispatch<SetStateAction<number>>;
  readonly maximumEndingBalance: number | null;
  readonly setMaximumEndingBalance: Dispatch<SetStateAction<number | null>>;
  readonly allowExpectedContributionAboveMaximum: boolean;
  readonly setAllowExpectedContributionAboveMaximum: Dispatch<
    SetStateAction<boolean>
  >;
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
  const [minimumEndingBalance, setMinimumEndingBalance] = useState(0);
  const [maximumEndingBalance, setMaximumEndingBalance] = useState<
    number | null
  >(null);
  const [
    allowExpectedContributionAboveMaximum,
    setAllowExpectedContributionAboveMaximum,
  ] = useState(false);
  const reset = (): void => {
    setName("");
    setDescription("");
    setPlannedMonthlyContribution(null);
    setMinimumEndingBalance(0);
    setMaximumEndingBalance(null);
    setAllowExpectedContributionAboveMaximum(false);
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
    allowExpectedContributionAboveMaximum,
    setAllowExpectedContributionAboveMaximum,
    reset,
  };
};

export type { FundSetupState };
export default useFundSetupState;
