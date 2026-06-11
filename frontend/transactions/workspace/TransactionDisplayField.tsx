import { Stack, TextField, Typography } from "@mui/material";
import type { JSX } from "react";

interface TransactionDisplayFieldProps {
  readonly label: string;
  readonly value: string;
  readonly helperText?: string | null;
}

/**
 * Displays a read-only transaction field using the same input styling as editable forms.
 */
const TransactionDisplayField = function ({
  label,
  value,
  helperText = null,
}: TransactionDisplayFieldProps): JSX.Element {
  return (
    <Stack spacing={0.75}>
      <TextField
        label={label}
        value={value}
        variant="outlined"
        slotProps={{
          input: {
            readOnly: true,
          },
        }}
      />
      {helperText !== null ? (
        <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
          {helperText}
        </Typography>
      ) : null}
    </Stack>
  );
};

export default TransactionDisplayField;
