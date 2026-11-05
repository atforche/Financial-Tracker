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
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Period</TableCell>
              <TableCell align="right">Minimum</TableCell>
              <TableCell align="right">Maximum</TableCell>
              <TableCell align="right">Balance</TableCell>
              <TableCell align="right">Change</TableCell>
              {showContribution ? (
                <TableCell align="right">Contribution</TableCell>
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
                <TableCell align="right">
                  {point.minimum === null ? "—" : formatCurrency(point.minimum)}
                </TableCell>
                <TableCell align="right">
                  {point.minimum === null
                    ? "—"
                    : point.maximum === null
                      ? "No maximum"
                      : formatCurrency(point.maximum)}
                </TableCell>
                <TableCell align="right">
                  {point.balance === null ? "—" : formatCurrency(point.balance)}
                </TableCell>
                <TableCell align="right">
                  {point.change === null ? (
                    "—"
                  ) : (
                    <SignedChange amount={point.change} />
                  )}
                </TableCell>
                {showContribution ? (
                  <TableCell align="right">
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
