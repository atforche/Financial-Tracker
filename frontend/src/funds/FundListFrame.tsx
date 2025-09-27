import { AddCircleOutline, Info, ModeEdit } from "@mui/icons-material";
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { type Fund, getAllFunds } from "@data/FundRepository";
import Action from "@framework/listframe/Action";
import ActionColumn from "@framework/listframe/ActionColumn";
import type Column from "@framework/listframe/Column";
import FundDialog from "@funds/FundDialog";
import HeaderAction from "@framework/listframe/HeaderAction";
import ModifyFundDialog from "@funds/ModifyFundDialog";
import StringColumn from "@framework/listframe/StringColumn";
import { useQuery } from "@data/useQuery";
import { useState } from "react";

/**
 * Component that provides a list of funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns {JSX.Element} JSX element representing a list of funds with various action buttons.
 */
const FundListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);

  const { data, refetch } = useQuery<Fund[]>({
    queryFunction: getAllFunds,
    initialData: [],
  });

  const columns: Column<Fund>[] = [
    new StringColumn<Fund>("Name", (value) => value.name),
    new StringColumn<Fund>("Description", (value) => value.description),
    new ActionColumn<Fund>(
      new HeaderAction(<AddCircleOutline />, "Add", () => {
        setDialog(
          <ModifyFundDialog
            fund={null}
            onClose={() => {
              setDialog(null);
              refetch();
            }}
          />,
        );
      }),
      [
        new Action<Fund>(<Info />, (row: Fund) => () => {
          setDialog(
            <FundDialog
              fund={row}
              onClose={() => {
                setDialog(null);
              }}
            />,
          );
        }),
        new Action<Fund>(<ModeEdit />, (row: Fund) => () => {
          setDialog(
            <ModifyFundDialog
              fund={row}
              onClose={() => {
                setDialog(null);
                refetch();
              }}
            />,
          );
        }),
      ],
    ),
  ];

  return (
    <Box sx={{ paddingLeft: "25px" }}>
      <Typography
        variant="h6"
        sx={{ paddingBottom: "25px", paddingTop: "25px" }}
      >
        Funds
      </Typography>
      <Paper
        sx={{
          [`& .MuiIconButton-root`]: {
            display: "none",
          },
          [`& .MuiTableRow-hover:hover`]: {
            [`.MuiIconButton-root`]: {
              display: "inline-block",
            },
          },
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((column) => column.getHeaderElement())}
              </TableRow>
            </TableHead>
            <TableBody>
              {data.map((row) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={row.name}>
                  {columns.map((column) => column.getRowElement(row))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
      {dialog}
    </Box>
  );
};

export default FundListFrame;
