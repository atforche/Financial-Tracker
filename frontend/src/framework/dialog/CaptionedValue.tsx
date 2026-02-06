import { Stack, Typography } from "@mui/material";
import Caption from "@framework/dialog/Caption";
import type { JSX } from "react";

/**
 * Props for the CaptionedValue component.
 */
interface CaptionedValueProps {
  readonly caption: string;
  readonly value: string | JSX.Element;
}

/**
 * Component that presents the user with a caption and its corresponding value.
 * @param props - Props for the CaptionedValue component.
 * @returns JSX element representing the CaptionedValue component.
 */
const CaptionedValue = function ({
  caption,
  value,
}: CaptionedValueProps): JSX.Element {
  return (
    <Stack direction="row" justifyContent="space-between">
      <Caption caption={caption} />
      <Typography variant="subtitle1">{value}</Typography>
    </Stack>
  );
};

export default CaptionedValue;
