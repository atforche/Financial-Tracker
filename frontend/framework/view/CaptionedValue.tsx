import { Stack, Typography } from "@mui/material";
import Caption from "@/framework/view/Caption";
import type { JSX } from "react";

const defaultMaxWidth = 500;

/**
 * Props for the CaptionedValue component.
 */
interface CaptionedValueProps {
  readonly caption: string;
  readonly value: string | JSX.Element;
}

/**
 * Component that presents the user with a caption and its corresponding value.
 */
const CaptionedValue = function ({
  caption,
  value,
}: CaptionedValueProps): JSX.Element {
  return (
    <Stack
      direction="row"
      justifyContent="space-between"
      sx={{ maxWidth: defaultMaxWidth }}
    >
      <Caption caption={caption} />
      <Typography variant="subtitle1">{value}</Typography>
    </Stack>
  );
};

export default CaptionedValue;
