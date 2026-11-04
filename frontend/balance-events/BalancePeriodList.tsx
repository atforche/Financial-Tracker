import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import SignedChange from "@/framework/view/SignedChange";
import accountingPeriodRoutes from "@/accounting-periods/routes";

/**
 * One accounting period's balances for an account or fund.
 */
interface BalancePeriod {
  readonly id: string;
  readonly name: string;
  readonly openingBalance: number;
  readonly closingBalance: number;
}

/**
 * Lists how an account or fund's balance changed in each accounting period of a range. This is a plain table because
 * it shares the page with a list frame, and only one list frame can own the page size.
 */
const BalancePeriodList = function ({
  periods,
}: {
  readonly periods: readonly BalancePeriod[];
}): JSX.Element {
  return (
    <ContentSurface>
      <Typography variant="h5" sx={{ mb: 1 }}>
        Periods
      </Typography>
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Period</TableCell>
              <TableCell align="right">Opening</TableCell>
              <TableCell align="right">Closing</TableCell>
              <TableCell align="right">Change</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {periods.map((period) => (
              <TableRow key={period.id}>
                <TableCell>
                  <Link
                    href={accountingPeriodRoutes.workspaceDetail(period.id, {})}
                  >
                    {period.name}
                  </Link>
                </TableCell>
                <TableCell align="right">
                  {formatCurrency(period.openingBalance)}
                </TableCell>
                <TableCell align="right">
                  {formatCurrency(period.closingBalance)}
                </TableCell>
                <TableCell align="right">
                  <SignedChange
                    amount={getCurrencyDifference(
                      period.closingBalance,
                      period.openingBalance,
                    )}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </ContentSurface>
  );
};

export default BalancePeriodList;
