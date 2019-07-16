# XML Mapping

The <Map> class allows the user to map fields between two XML files. This way the user can fill/modify an XML file's fields using another XML file's fields with desired values. It comes with a Log class that keeps track of errors and progress.

Use Map class method public static void FillXML(XDocument sourceXML, XDocument destinationXML) to copy values from the <sourceXML> to the <destinationXML>.
