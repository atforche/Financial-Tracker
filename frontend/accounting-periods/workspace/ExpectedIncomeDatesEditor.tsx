import { Box, Stack, Typography } from "@mui/material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { JSX } from "react";
import dayjs from "dayjs";

/**
 * Props for the ExpectedIncomeDatesEditor component.
 */
interface ExpectedIncomeDatesEditorProps {
  readonly dates: string[];
  readonly setDates: (dates: string[]) => void;
  readonly year: number;
  readonly month: number;
}

/**
 * Edits the dates an expected-income source is expected to pay, limited to its accounting period's month.
 */
const ExpectedIncomeDatesEditor = function ({
  dates,
  setDates,
  year,
  month,
}: ExpectedIncomeDatesEditorProps): JSX.Element {
  const minDate = dayjs(new Date(year, month - 1, 1));
  const maxDate = minDate.endOf("month");
  return (
    <Stack spacing={1.5}>
      <Stack spacing={0.25}>
        <Typography variant="subtitle2">Expected Payment Dates</Typography>
        <Typography variant="body2" color="text.secondary">
          Add the dates on which this source is expected to pay.
        </Typography>
      </Stack>
      <CollectionEditor
        items={dates}
        label="Expected Payment Dates"
        setItems={setDates}
        createItem={() => minDate.format("YYYY-MM-DD")}
        addLabel="Add Expected Date"
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
              gridTemplateColumns: "minmax(0, 1fr) auto",
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
              setValue={(value): void => {
                if (value === null) {
                  setDates(dates.filter((_, itemIndex) => itemIndex !== index));
                } else if (value.isValid()) {
                  setDates(
                    dates.map((item, itemIndex) =>
                      itemIndex === index ? value.format("YYYY-MM-DD") : item,
                    ),
                  );
                }
              }}
            />
            <Box sx={{ display: "flex", justifyContent: "center", pt: 1.25 }}>
              {controls.deleteButton}
            </Box>
          </Box>
        )}
      />
    </Stack>
  );
};

export default ExpectedIncomeDatesEditor;
