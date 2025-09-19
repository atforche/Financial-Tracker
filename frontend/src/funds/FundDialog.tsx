import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import { type Fund, getFundById } from "@data/FundRepository";
import StringEntryField from "@framework/dialog/StringEntryField";
import { useCallback } from "react";
import { useQuery } from "@data/useQuery";

/**
 * Props for the FundDialog component.
 * @param {string} fundId - ID of a Fund to display in this dialog.
 * @param {Function} onClose - Callback to perform when this dialog is closed.
 */
interface FundDialogProps {
  fundId: string;
  onClose: () => void;
}

/**
 * Component that presents the user with a dialog to view a Fund.
 * @param {FundDialogProps} props - Props for the FundDialog component.
 * @returns {JSX.Element} JSX element representing the FundDialog.
 * @throws An error if the Fund ID doesn't point to a valid fund.
 */
const FundDialog = function ({
  fundId,
  onClose,
}: FundDialogProps): JSX.Element {
  const fetchFund = useCallback(async () => getFundById(fundId), [fundId]);
  const { data } = useQuery<Fund | null>({
    queryFunction: fetchFund,
    initialData: null,
  });

  return (
    <Dialog open={true}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        View Fund
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          <>
            <StringEntryField label="Name" value={data?.name ?? ""} />
            <StringEntryField
              label="Description"
              value={data?.description ?? ""}
            />
          </>
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={onClose}>Close</Button>
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default FundDialog;
