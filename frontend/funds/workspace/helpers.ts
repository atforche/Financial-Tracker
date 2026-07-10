import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
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
 * Validates a request to create a fund.
 */
const validateCreateFundRequest = function (
  name: string,
  accountingPeriod: AccountingPeriod | null,
  assignmentGoalType: AssignmentGoalType | null,
  assignmentGoalAmount: number | null,
  spendingGoalType: SpendingGoalType | null,
): boolean {
  return (
    validateCreateFundSetup(name, accountingPeriod) &&
    validateAssignmentGoalSetup(assignmentGoalType, assignmentGoalAmount) &&
    validateSpendingGoalSetup(spendingGoalType)
  );
};

/**
 * Builds a request to create a fund.
 */
const buildCreateFundRequest = function (
  name: string,
  description: string,
  accountingPeriod: AccountingPeriod | null,
  assignmentGoalType: AssignmentGoalType | null,
  assignmentGoalAmount: number | null,
  spendingGoalType: SpendingGoalType | null,
): CreateFundRequest | null {
  if (
    !validateCreateFundRequest(
      name,
      accountingPeriod,
      assignmentGoalType,
      assignmentGoalAmount,
      spendingGoalType,
    )
  ) {
    return null;
  }

  return {
    name,
    description,
    accountingPeriodId: accountingPeriod?.id ?? "",
    assignmentGoalType: assignmentGoalType ?? AssignmentGoalType.MonthlyTarget,
    assignmentGoalAmount: assignmentGoalAmount ?? 0,
    spendingGoalType: spendingGoalType ?? SpendingGoalType.Standard,
  };
};

/**
 * Validates a request to onboard a fund.
 */
const validateOnboardFundRequest = function (
  name: string,
  onboardedBalance: number | null,
  assignmentGoalType: AssignmentGoalType | null,
  assignmentGoalAmount: number | null,
  spendingGoalType: SpendingGoalType | null,
): boolean {
  return (
    validateOnboardFundSetup(name, onboardedBalance) &&
    validateAssignmentGoalSetup(assignmentGoalType, assignmentGoalAmount) &&
    validateSpendingGoalSetup(spendingGoalType)
  );
};

/**
 * Builds a request to onboard a fund.
 */
const buildOnboardFundRequest = function (
  name: string,
  description: string,
  onboardedBalance: number | null,
  assignmentGoalType: AssignmentGoalType | null,
  assignmentGoalAmount: number | null,
  spendingGoalType: SpendingGoalType | null,
): OnboardFundRequest | null {
  if (
    !validateOnboardFundRequest(
      name,
      onboardedBalance,
      assignmentGoalType,
      assignmentGoalAmount,
      spendingGoalType,
    )
  ) {
    return null;
  }

  return {
    name,
    description,
    onboardedBalance: onboardedBalance ?? 0,
    assignmentGoalType: assignmentGoalType ?? AssignmentGoalType.MonthlyTarget,
    assignmentGoalAmount: assignmentGoalAmount ?? 0,
    spendingGoalType: spendingGoalType ?? SpendingGoalType.Standard,
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
  validateCreateFundRequest,
  buildCreateFundRequest,
  validateOnboardFundRequest,
  buildOnboardFundRequest,
  getAssignmentAmountHelperText,
};
