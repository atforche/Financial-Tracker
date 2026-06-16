import type { JSX, ReactNode } from "react";
import { Stack, TextField, Typography } from "@mui/material";

interface TransactionDisplayFieldProps {
  readonly label: string;
  readonly value: string;
  readonly helperText?: ReactNode;
}

/**
 * Displays a read-only transaction field using the same input styling as editable forms.
 */
const TransactionDisplayField = function ({
  label,
  value,
  helperText = null,
}: TransactionDisplayFieldProps): JSX.Element {
  let resolvedHelperText = helperText;

  if (typeof helperText === "string") {
    resolvedHelperText = (
      <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
        {helperText}
      </Typography>
    );
  }

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
      {resolvedHelperText}
    </Stack>
  );
};

export default TransactionDisplayField;
