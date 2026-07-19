import { AssignmentGoalType, type SpendingGoalType } from "@/goals/types";
import type { CreateFundRequest, OnboardFundRequest } from "@/funds/types";
import type { AccountingPeriod } from "@/accounting-periods/types";

/**
 * Validates the fund setup section for fund creation.
 */
const validateCreateFundSetup = function (
  name: string,
  accountingPeriod: AccountingPeriod | null,
): boolean {
  return name !== "" && accountingPeriod !== null;
};

/**
 * Validates the fund setup section for fund onboarding.
 */
const validateOnboardFundSetup = function (
  name: string,
  onboardedBalance: number | null,
): boolean {
  return name !== "" && onboardedBalance !== null;
};

/**
 * Validates the assignment goal setup section.
 */
const validateAssignmentGoalSetup = function (
  assignmentGoalType: AssignmentGoalType | null,
  assignmentGoalAmount: number | null,
): boolean {
  return assignmentGoalType !== null && assignmentGoalAmount !== null;
};

/**
 * Validates the spending goal setup section.
 */
const validateSpendingGoalSetup = function (
  spendingGoalType: SpendingGoalType | null,
): boolean {
  return spendingGoalType !== null;
};

/**
 * Fields required to build a request to create a fund.
 */
interface CreateFundRequestFields {
  readonly name: string;
  readonly description: string;
  readonly accountingPeriod: AccountingPeriod | null;
  readonly assignmentGoalType: AssignmentGoalType | null;
  readonly assignmentGoalAmount: number | null;
  readonly spendingGoalType: SpendingGoalType | null;
}

/**
 * Builds a request to create a fund.
 */
const buildCreateFundRequest = function (
  fields: CreateFundRequestFields,
): CreateFundRequest | null {
  const {
    name,
    description,
    accountingPeriod,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  } = fields;
  if (
    name === "" ||
    accountingPeriod === null ||
    assignmentGoalType === null ||
    assignmentGoalAmount === null ||
    spendingGoalType === null
  ) {
    return null;
  }

  return {
    name,
    description,
    accountingPeriodId: accountingPeriod.id,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  };
};

/**
 * Fields required to build a request to onboard a fund.
 */
interface OnboardFundRequestFields {
  readonly name: string;
  readonly description: string;
  readonly onboardedBalance: number | null;
  readonly assignmentGoalType: AssignmentGoalType | null;
  readonly assignmentGoalAmount: number | null;
  readonly spendingGoalType: SpendingGoalType | null;
}

/**
 * Builds a request to onboard a fund.
 */
const buildOnboardFundRequest = function (
  fields: OnboardFundRequestFields,
): OnboardFundRequest | null {
  const {
    name,
    description,
    onboardedBalance,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  } = fields;
  if (
    name === "" ||
    onboardedBalance === null ||
    assignmentGoalType === null ||
    assignmentGoalAmount === null ||
    spendingGoalType === null
  ) {
    return null;
  }

  return {
    name,
    description,
    onboardedBalance,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  };
};

/**
 * Gets the assignment amount helper text for the provided goal type.
 */
const getAssignmentAmountHelperText = function (
  goalType: AssignmentGoalType | null,
): string {
  if (goalType === null) {
    return "Choose the assignment behavior that matches how you want to fund this category.";
  }
  switch (goalType) {
    case AssignmentGoalType.MonthlyTarget:
      return "Enter the ending balance you want this fund to reach for the period.";
    case AssignmentGoalType.RecurringContribution:
      return "Enter the amount you want to assign into this fund during the period.";
    default:
      return goalType satisfies never;
  }
};

export {
  validateCreateFundSetup,
  validateOnboardFundSetup,
  validateAssignmentGoalSetup,
  validateSpendingGoalSetup,
  buildCreateFundRequest,
  buildOnboardFundRequest,
  getAssignmentAmountHelperText,
};
