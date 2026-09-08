import { Box, Stack, Typography } from "@mui/material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { JSX } from "react";
import dayjs from "dayjs";

interface Props {
  readonly dates: string[];
  readonly setDates?: ((dates: string[]) => void) | null;
  readonly year: number | null;
  readonly month: number | null;
}

/** Displays expected payment dates in editable or read-only form. */
const ExpectedIncomeDatesEditor = function ({
  dates,
  setDates = null,
  year,
  month,
}: Props): JSX.Element {
  const readOnly = setDates === null;
  const minDate =
    year === null || month === null
      ? null
      : dayjs(new Date(year, month - 1, 1));
  const maxDate = minDate?.endOf("month") ?? null;
  return (
    <Stack spacing={1.5}>
      {!readOnly || dates.length <= 1 ? (
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">Expected Payment Dates</Typography>
          <Typography variant="body2" color="text.secondary">
            Add the dates on which this source is expected to pay.
          </Typography>
        </Stack>
      ) : null}
      {readOnly && dates.length === 0 ? (
        <Typography color="text.secondary">No items available.</Typography>
      ) : null}
      <CollectionEditor
        items={dates}
        label="Expected Payment Dates"
        readOnly={readOnly}
        setItems={setDates}
        createItem={() => minDate?.format("YYYY-MM-DD") ?? ""}
        addLabel="Add Expected Date"
        showAddButton={minDate !== null}
        itemContainerSx={{
          display: "grid",
          gap: 1.5,
          gridTemplateColumns: "repeat(auto-fit, minmax(260px, 1fr))",
        }}
        renderItem={(date, index, controls) => (
          <Box
            key={index}
            sx={{
              display: "grid",
              gap: 1.5,
              gridTemplateColumns: {
                xs: "1fr",
                md: readOnly ? "1fr" : "minmax(0, 1fr) auto",
              },
              alignItems: "start",
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 2,
              p: 1.5,
            }}
          >
            <DateEntryField
              label="Expected Date"
              value={dayjs(date)}
              minDate={minDate}
              maxDate={maxDate}
              autoFocus={controls.autoFocus}
              setValue={
                readOnly
                  ? null
                  : (value): void => {
                      if (value === null) {
                        setDates(
                          dates.filter((_, itemIndex) => itemIndex !== index),
                        );
                      } else if (value.isValid()) {
                        setDates(
                          dates.map((item, itemIndex) =>
                            itemIndex === index
                              ? value.format("YYYY-MM-DD")
                              : item,
                          ),
                        );
                      }
                    }
              }
            />
            {readOnly ? null : (
              <Box
                sx={{
                  display: "flex",
                  justifyContent: { xs: "flex-end", md: "center" },
                  pt: { xs: 0, md: 1.25 },
                }}
              >
                {controls.deleteButton}
              </Box>
            )}
          </Box>
        )}
      />
    </Stack>
  );
};

export default ExpectedIncomeDatesEditor;
