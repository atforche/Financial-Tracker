import { Box, Typography } from "@mui/material";
import type { JSX } from "react";

const defaultMinWidth = 300;
const defaultMaxWidth = 500;

/**
 * Props for CaptionedFrame component.
 */
interface CaptionedFrameProps {
  readonly caption: string;
  readonly minWidth?: number;
  readonly maxWidth?: number | null;
  readonly children: React.ReactNode;
}

/**
 * Component that presents the user with a frame that has a caption and children content.
 */
const CaptionedFrame = function ({
  caption,
  minWidth = defaultMinWidth,
  maxWidth = defaultMaxWidth,
  children,
}: CaptionedFrameProps): JSX.Element {
  return (
    <Box
      component="fieldset"
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        padding: "0 15px 15px",
        width: maxWidth === null ? "fit-content" : undefined,
        minWidth,
        maxWidth: maxWidth ?? "fit-content",
      }}
    >
      <legend>
        <Typography variant="subtitle1">{caption}</Typography>
      </legend>
      {children}
    </Box>
  );
};

export default CaptionedFrame;
