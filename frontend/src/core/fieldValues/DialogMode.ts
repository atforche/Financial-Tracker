import FieldValue from "@core/fieldValues/FieldValue";

/**
 * DialogMode field value that represents the different modes a dialog can be opened in.
 */
class DialogMode extends FieldValue {
  /**
   * A dialog open in Create mode can add a new entity.
   */
  public static readonly Create = new DialogMode("Create");

  /**
   * A dialog open in View mode can view an existing entity in read-only mode.
   */
  public static readonly View = new DialogMode("View");

  /**
   * A dialog open in Update mode can modify values on an existing entity.
   */
  public static readonly Update = new DialogMode("Update");

  /**
   * A dialog open in Delete mode will delete an existing entity.
   */
  public static readonly Delete = new DialogMode("Delete");

  /**
   * Collection of all DialogModes.
   */
  public static override readonly Collection = [
    DialogMode.Create,
    DialogMode.View,
    DialogMode.Update,
    DialogMode.Delete,
  ];
}

export default DialogMode;
