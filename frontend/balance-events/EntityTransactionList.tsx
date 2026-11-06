"use client";

import {
  Box,
  ButtonBase,
  Collapse,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { type JSX, useState } from "react";
import getTransactionRow, {
  type EntityEvent,
} from "@/balance-events/getTransactionRow";
import ContentSurface from "@/framework/view/ContentSurface";
import ExpandMore from "@mui/icons-material/ExpandMore";
import RowList from "@/framework/view/RowList";
import dayjs from "dayjs";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";

/**
 * Posted events in one calendar month.
 */
interface MonthGroup {
  readonly key: string;
  readonly events: EntityEvent[];
}

/**
 * Groups posted events by month, keeping their newest-first order.
 */
const groupByMonth = function (events: readonly EntityEvent[]): MonthGroup[] {
  const groups = new Map<string, MonthGroup>();
  events.forEach((event) => {
    const key = (event.eventDate ?? event.transactionDate).slice(0, 7);
    const group = groups.get(key) ?? { key, events: [] };
    group.events.push(event);
    groups.set(key, group);
  });
  return [...groups.values()].sort((first, second) =>
    second.key.localeCompare(first.key),
  );
};

/**
 * Lists how transactions affected an account or fund across a date range: pending ones first, then posted ones by
 * month. Only the newest month starts open, and a search opens every month with a match.
 */
const EntityTransactionList = function ({
  entityName,
  events,
  returnUrl,
}: {
  readonly entityName: "account" | "fund";
  readonly events: readonly EntityEvent[];
  readonly returnUrl: string;
}): JSX.Element {
  const [search, setSearch] = useState("");
  const [toggledMonths, setToggledMonths] = useState<ReadonlySet<string>>(
    new Set(),
  );
  const term = search.trim().toLowerCase();
  const matches = events.filter(
    (event) =>
      event.description.toLowerCase().includes(term) ||
      formatBalanceEventCounterparty(event).toLowerCase().includes(term),
  );
  const pending = matches.filter((event) => !event.isPosted);
  const months = groupByMonth(matches.filter((event) => event.isPosted));
  const newestMonth = groupByMonth(events.filter((event) => event.isPosted)).at(
    0,
  )?.key;
  const toggleMonth = function (key: string): void {
    setToggledMonths((current) => {
      const next = new Set(current);
      if (!next.delete(key)) {
        next.add(key);
      }
      return next;
    });
  };
  const getRows = (
    items: readonly EntityEvent[],
  ): ReturnType<typeof getTransactionRow>[] =>
    items.map((event, index) => getTransactionRow(event, index, returnUrl));

  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          sx={{
            alignItems: { xs: "stretch", sm: "center" },
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h5">Transactions</Typography>
          <TextField
            label="Search transactions"
            size="small"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value);
            }}
          />
        </Stack>
        {matches.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            {term === ""
              ? `No transactions affected this ${entityName} in this range.`
              : "No transactions match this search."}
          </Typography>
        ) : null}
        {pending.length === 0 ? null : (
          <Box>
            <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
              Pending
            </Typography>
            <RowList rows={getRows(pending)} initialRowCount={pending.length} />
          </Box>
        )}
        {months.map((month) => {
          const isOpen =
            term !== "" ||
            (month.key === newestMonth) !== toggledMonths.has(month.key);
          return (
            <Box key={month.key}>
              <ButtonBase
                aria-expanded={isOpen}
                disabled={term !== ""}
                onClick={() => {
                  toggleMonth(month.key);
                }}
                sx={{
                  display: "flex",
                  gap: 2,
                  justifyContent: "flex-start",
                  py: 0.75,
                  textAlign: "left",
                  width: "100%",
                }}
              >
                <ExpandMore
                  fontSize="small"
                  sx={{ transform: isOpen ? "none" : "rotate(-90deg)" }}
                />
                <Typography
                  variant="subtitle1"
                  sx={{ flexGrow: 1, fontWeight: 600 }}
                >
                  {dayjs(`${month.key}-01`).format("MMMM YYYY")}
                </Typography>
              </ButtonBase>
              <Collapse in={isOpen} unmountOnExit>
                <RowList
                  rows={getRows(month.events)}
                  initialRowCount={month.events.length}
                />
              </Collapse>
            </Box>
          );
        })}
      </Stack>
    </ContentSurface>
  );
};

export default EntityTransactionList;
