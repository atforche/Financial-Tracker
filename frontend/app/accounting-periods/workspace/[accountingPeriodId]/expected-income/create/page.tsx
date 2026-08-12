import ExpectedIncomeSourcePage, {
  type ExpectedIncomeSourcePageRouteProps,
} from "@/accounting-periods/workspace/ExpectedIncomeSourcePage";
import type { JSX } from "react";

/** Displays the create expected-income source page. */
const CreateExpectedIncomeSourcePage = function (
  props: ExpectedIncomeSourcePageRouteProps,
): JSX.Element {
  return <ExpectedIncomeSourcePage {...props} mode="add" />;
};

export const dynamic = "force-dynamic";
export default CreateExpectedIncomeSourcePage;
