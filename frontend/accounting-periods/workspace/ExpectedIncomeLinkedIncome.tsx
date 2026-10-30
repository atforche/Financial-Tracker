import { Divider, Stack, Typography } from "@mui/material";
import { Fragment, type JSX } from "react";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { ExpectedIncomeSourceIncome } from "@/accounting-periods/types";
import Link from "next/link";
import { formatShortDate } from "@/framework/dateHelpers";
import transactionRoutes from "@/transactions/routes";

/**
 * Props for the ExpectedIncomeLinkedIncome component.
 */
interface ExpectedIncomeLinkedIncomeProps {
  readonly income: readonly ExpectedIncomeSourceIncome[];
  readonly returnUrl: string;
}

/**
 * Lists the income transactions linked to an expected-income source, with the amount of each still pending.
 */
const ExpectedIncomeLinkedIncome = function ({
  income,
  returnUrl,
}: ExpectedIncomeLinkedIncomeProps): JSX.Element {
  return (
    <Stack>
      <Typography variant="body2" fontWeight={600} sx={{ pb: 0.5 }}>
        Linked income
      </Typography>
      {income.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No income has been linked to this source yet.
        </Typography>
      ) : (
        income.map((item, index) => (
          <Fragment key={item.transactionId}>
            {index > 0 && <Divider />}
            <Stack
              component={Link}
              href={transactionRoutes.workspaceDetail(item.transactionId, {
                returnUrl,
              })}
              direction="row"
              spacing={2}
              sx={{
                justifyContent: "space-between",
                py: 1,
                color: "inherit",
                textDecoration: "none",
              }}
            >
              <Stack sx={{ minWidth: 0 }}>
                <Typography variant="body2" fontWeight={600} noWrap>
                  {item.description}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {formatShortDate(new Date(`${item.date}T00:00:00`))}
                </Typography>
              </Stack>
              <Stack sx={{ alignItems: "flex-end" }}>
                <Typography variant="body2" fontWeight={600}>
                  {formatCurrency(item.amount)}
                </Typography>
                {item.receivedAmount.total !== item.amount && (
                  <Typography variant="caption" color="warning.main">
                    {formatCurrency(
                      getCurrencyDifference(
                        item.amount,
                        item.receivedAmount.total,
                      ),
                    )}{" "}
                    pending
                  </Typography>
                )}
              </Stack>
            </Stack>
          </Fragment>
        ))
      )}
    </Stack>
  );
};

export default ExpectedIncomeLinkedIncome;
