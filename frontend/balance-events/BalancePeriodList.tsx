"use client";

import {
  Box,
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
import { linkedRowSx, rowLinkSx } from "@/framework/view/linkedRowSx";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";

/**
 * One accounting period's balances for an account or fund.
 */
interface BalancePeriod {
  readonly id: string;
  readonly name: string;
  readonly href: Route;
  readonly openingBalance: number;
  readonly closingBalance: number;
}

/**
 * Lists how an account's balance changed in each accounting period of a range, for accounts without goals.
 */
const BalancePeriodList = function ({
  periods,
  isIncreaseGood = true,
}: {
  readonly periods: readonly BalancePeriod[];
  readonly isIncreaseGood?: boolean;
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
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {periods.map((period) => (
              <TableRow key={period.id} sx={linkedRowSx}>
                <TableCell>
                  <Box component={Link} href={period.href} sx={rowLinkSx}>
                    {period.name}
                  </Box>
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
                    isIncreaseGood={isIncreaseGood}
                  />
                </TableCell>
                <TableCell align="right">
                  <ChevronRight
                    fontSize="small"
                    sx={{
                      color: "text.secondary",
                      display: "block",
                      ml: "auto",
                    }}
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
