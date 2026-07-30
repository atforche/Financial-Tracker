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
  readonly regularContribution: number | null;
  readonly setRegularContribution: Dispatch<SetStateAction<number | null>>;
  readonly minimumFundedBalance: number | null;
  readonly setMinimumFundedBalance: Dispatch<SetStateAction<number | null>>;
  readonly maximumFundedBalance: number | null;
  readonly setMaximumFundedBalance: Dispatch<SetStateAction<number | null>>;
  readonly targetEndingBalance: number | null;
  readonly setTargetEndingBalance: Dispatch<SetStateAction<number | null>>;
  readonly reset: () => void;
}

/**
 * Owns the fields shared by the create and onboarding fund forms.
 */
const useFundSetupState = function (): FundSetupState {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [regularContribution, setRegularContribution] = useState<number | null>(
    null,
  );
  const [minimumFundedBalance, setMinimumFundedBalance] = useState<
    number | null
  >(null);
  const [maximumFundedBalance, setMaximumFundedBalance] = useState<
    number | null
  >(null);
  const [targetEndingBalance, setTargetEndingBalance] = useState<number | null>(
    null,
  );
  const reset = (): void => {
    setName("");
    setDescription("");
    setRegularContribution(null);
    setMinimumFundedBalance(null);
    setMaximumFundedBalance(null);
    setTargetEndingBalance(null);
  };
  return {
    name,
    setName,
    description,
    setDescription,
    regularContribution,
    setRegularContribution,
    minimumFundedBalance,
    setMinimumFundedBalance,
    maximumFundedBalance,
    setMaximumFundedBalance,
    targetEndingBalance,
    setTargetEndingBalance,
    reset,
  };
};

export type { FundSetupState };
export default useFundSetupState;
