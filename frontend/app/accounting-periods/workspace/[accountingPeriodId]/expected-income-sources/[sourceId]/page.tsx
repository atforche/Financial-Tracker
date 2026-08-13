import ExpectedIncomeSourcePage from "@/accounting-periods/workspace/ExpectedIncomeSourcePage";
import type { JSX } from "react";

/**
 * Props for the Page component, which displays an expected-income source page.
 */
interface PageProps {
  readonly params: Promise<{ accountingPeriodId: string; sourceId: string }>;
  readonly searchParams: Promise<{ returnUrl?: string }>;
}

/**
 * Displays an expected-income source page, which can be in either view or edit mode, depending on the provided props.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId, sourceId } = await params;
  const { returnUrl } = await searchParams;
  return (
    <ExpectedIncomeSourcePage
      accountingPeriodId={accountingPeriodId}
      sourceId={sourceId}
      mode="view"
      returnUrl={returnUrl}
    />
  );
};

export default Page;
export const dynamic = "force-dynamic";
