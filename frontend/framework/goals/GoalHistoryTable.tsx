import {
  Box,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { linkedRowSx, rowLinkSx } from "@/framework/view/linkedRowSx";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * A single Accounting Period row in a goal history table.
 */
interface HistoryPoint {
  readonly periodId: string;
  readonly href: Route;
  readonly period: string;
  readonly periodOrder: number;
  readonly isOpen: boolean;
  readonly minimum: number | null;
  readonly maximum: number | null;
  readonly span: number | null;
  readonly balance: number | null;
  readonly change: number | null;
  readonly expected: number | null;
  readonly assigned: number | null;
  readonly satisfied: boolean | null;
}

/**
 * Hides a column on phones, leaving the period, balance, and result. The goal range is drawn in the chart above.
 */
const phoneHiddenSx = { display: { xs: "none", sm: "table-cell" } } as const;

/**
 * Describes how a period's goal turned out.
 */
const getResultLabel = function (point: HistoryPoint): string {
  if (point.satisfied === null) {
    return "No goal";
  }
  if (point.isOpen) {
    return point.satisfied ? "On track" : "Outside range";
  }
  return point.satisfied ? "Achieved" : "Missed";
};

/**
 * Lists each accounting period's goal and result, newest first.
 */
const GoalHistoryTable = function ({
  points,
  showContribution,
}: {
  readonly points: readonly HistoryPoint[];
  readonly showContribution: boolean;
}): JSX.Element {
  const sorted = [...points].sort(
    (first, second) => second.periodOrder - first.periodOrder,
  );
  return (
    <ContentSurface>
      <Typography variant="h5" sx={{ mb: 1 }}>
        Periods
      </Typography>
      <TableContainer>
        <Table
          size="small"
          // Phones tighten the cell padding so the row chevron fits on screen
          sx={{
            "& .MuiTableCell-root": { px: { xs: 1, sm: 2 } },
            "& .MuiTableCell-root:last-of-type": { pr: { xs: 0, sm: 2 } },
          }}
        >
          <TableHead>
            <TableRow>
              <TableCell>Period</TableCell>
              <TableCell align="right" sx={phoneHiddenSx}>
                Minimum
              </TableCell>
              <TableCell align="right" sx={phoneHiddenSx}>
                Maximum
              </TableCell>
              <TableCell align="right">Balance</TableCell>
              <TableCell align="right" sx={phoneHiddenSx}>
                Change
              </TableCell>
              {showContribution ? (
                <TableCell align="right" sx={phoneHiddenSx}>
                  Contribution
                </TableCell>
              ) : null}
              <TableCell>Result</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {sorted.map((point) => (
              <TableRow key={point.periodId} sx={linkedRowSx}>
                <TableCell>
                  <Box component={Link} href={point.href} sx={rowLinkSx}>
                    {point.period}
                    {point.isOpen ? " (open)" : ""}
                  </Box>
                </TableCell>
                <TableCell align="right" sx={phoneHiddenSx}>
                  {point.minimum === null ? "—" : formatCurrency(point.minimum)}
                </TableCell>
                <TableCell align="right" sx={phoneHiddenSx}>
                  {point.minimum === null
                    ? "—"
                    : point.maximum === null
                      ? "No maximum"
                      : formatCurrency(point.maximum)}
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                  {point.balance === null ? "—" : formatCurrency(point.balance)}
                </TableCell>
                <TableCell align="right" sx={phoneHiddenSx}>
                  {point.change === null ? (
                    "—"
                  ) : (
                    <SignedChange amount={point.change} />
                  )}
                </TableCell>
                {showContribution ? (
                  <TableCell align="right" sx={phoneHiddenSx}>
                    {point.expected === null
                      ? "—"
                      : `${formatCurrency(point.assigned ?? 0)} / ${formatCurrency(point.expected)}`}
                  </TableCell>
                ) : null}
                <TableCell>
                  <Chip
                    size="small"
                    variant="outlined"
                    label={getResultLabel(point)}
                    color={
                      point.satisfied === null
                        ? "default"
                        : point.satisfied
                          ? "success"
                          : "error"
                    }
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

export type { HistoryPoint };
export default GoalHistoryTable;
