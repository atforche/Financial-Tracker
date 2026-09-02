import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import Button from "@mui/material/Button";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";

/**
 * Props for the AddCollectionItemButton component.
 */
interface AddCollectionItemButtonProps {
  readonly label: string;
  readonly onClick: () => void;
}

/**
 * Displays a subtle full-width action for adding an item to a collection.
 */
const AddCollectionItemButton = function ({
  label,
  onClick,
}: AddCollectionItemButtonProps): JSX.Element {
  return (
    <InsetFrame sx={{ p: 0.5 }}>
      <Button
        variant="text"
        fullWidth
        startIcon={<AddCircleOutline />}
        sx={{ justifyContent: "flex-start", py: 1 }}
        onClick={onClick}
      >
        {label}
      </Button>
    </InsetFrame>
  );
};

export default AddCollectionItemButton;
