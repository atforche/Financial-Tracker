import type { CreateFundRequest, OnboardFundRequest } from "@/funds/types";
import type { AccountingPeriod } from "@/accounting-periods/types";

/**
 * Validates the fund setup section for fund creation.
 */
const validateCreateFundSetup = (
  name: string,
  period: AccountingPeriod | null,
): boolean => name !== "" && period !== null;

/**
 * Validates the fund setup section for fund onboarding.
 */
const validateOnboardFundSetup = (
  name: string,
  balance: number | null,
): boolean => name !== "" && balance !== null;

/**
 * Fields required to build a Fund Goal.
 */
interface FundGoalFields {
  readonly regularContribution: number | null;
  readonly minimumFundedBalance: number | null;
  readonly maximumFundedBalance: number | null;
  readonly targetEndingBalance: number | null;
}

/**
 * Fields required to build a request to create a fund.
 */
interface CreateFundRequestFields extends FundGoalFields {
  readonly name: string;
  readonly description: string;
  readonly accountingPeriod: AccountingPeriod | null;
}

/**
 * Fields required to build a request to onboard a fund.
 */
interface OnboardFundRequestFields extends FundGoalFields {
  readonly name: string;
  readonly description: string;
  readonly onboardedBalance: number | null;
}

/**
 * Validates that the minimum and maximum funded balances are in a valid range.
 */
const validRange = (fields: FundGoalFields): boolean =>
  fields.minimumFundedBalance === null ||
  fields.maximumFundedBalance === null ||
  fields.minimumFundedBalance <= fields.maximumFundedBalance;

/**
 * Builds a request to create a fund.
 */
const buildCreateFundRequest = (
  fields: CreateFundRequestFields,
): CreateFundRequest | null => {
  if (
    !validateCreateFundSetup(fields.name, fields.accountingPeriod) ||
    !validRange(fields)
  ) {
    return null;
  }
  return {
    name: fields.name,
    description: fields.description,
    accountingPeriodId: fields.accountingPeriod?.id ?? "",
    regularContribution: fields.regularContribution,
    minimumFundedBalance: fields.minimumFundedBalance,
    maximumFundedBalance: fields.maximumFundedBalance,
    targetEndingBalance: fields.targetEndingBalance,
  };
};

/**
 * Fields required to build a request to onboard a fund.
 */
const buildOnboardFundRequest = (
  fields: OnboardFundRequestFields,
): OnboardFundRequest | null => {
  if (
    !validateOnboardFundSetup(fields.name, fields.onboardedBalance) ||
    !validRange(fields)
  ) {
    return null;
  }
  return {
    name: fields.name,
    description: fields.description,
    onboardedBalance: fields.onboardedBalance ?? 0,
    regularContribution: fields.regularContribution,
    minimumFundedBalance: fields.minimumFundedBalance,
    maximumFundedBalance: fields.maximumFundedBalance,
    targetEndingBalance: fields.targetEndingBalance,
  };
};

export {
  validateCreateFundSetup,
  validateOnboardFundSetup,
  buildCreateFundRequest,
  buildOnboardFundRequest,
};
