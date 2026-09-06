import { Box, Typography } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for CaptionedFrame component.
 */
interface CaptionedFrameProps {
  readonly caption: string;
  readonly children: React.ReactNode;
}

/**
 * Component that presents the user with a frame that has a caption and children content.
 */
const CaptionedFrame = function ({
  caption,
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
        width: "max-content",
        minWidth: 0,
        maxWidth: "100%",
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
