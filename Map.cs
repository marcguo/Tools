        /// <summary>
        /// Mapping between two XML files.
        /// </summary>
        private static class Map
        {
            /// <summary>
            /// Map File's location on disk.
            /// </summary>
            private static readonly string MAPFILEPATH
                = Directory.GetCurrentDirectory() + @"\Map\Map File.txt";
            /// <summary>
            /// Contain all mapping pairs in between the source XML
            /// and the destination XML files.
            /// </summary>
            private static List<Pair> pairs = new List<Pair>();

            /// <summary>
            /// Add mapping pairs from the Map file.
            /// </summary>
            private static void AddMapping()
            {
                // Open the map file and get all lines.
                if (File.Exists(MAPFILEPATH))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(MAPFILEPATH);
                        foreach (string line in lines)
                        {
                            string[] parts = line.Split(',');
                            string sourcePart = parts[0];
                            string destinationPart = parts[1];
                            List<XElement> source = Deserialise(sourcePart);
                            List<XElement> destination = Deserialise(destinationPart);
                            pairs.Add(new Pair(source, destination));
                        }
                    }
                    catch
                    {
                        Log.WriteLog("System - Please make sure the format of the Map file is correct.");
                        MessageBox.Show("Please make sure the format of the Map file is correct.");
                    }
                }
                else
                {
                    Log.WriteLog("System - The map file does not exist.");
                    MessageBox.Show("The map file does not exist.");
                }
            }

            /// <summary>
            /// Clear the existing mapping.
            /// </summary>
            private static void ClearMapping()
            {
                pairs.Clear();
            }

            /// <summary>
            /// Deserialise the mapping string.
            /// </summary>
            /// <param name="part">A part of the mapping string, either the source or the destination.</param>
            /// <returns>A list that contains the XML layers.</returns>
            private static List<XElement> Deserialise(string part)
            {
                List<XElement> output   = new List<XElement>();
                string[]       elements = part.Split('>');
                foreach (string element in elements)
                {
                    // If this element contains attributes:
                    if (element.Contains("."))
                    {
                        string[] subs = element.Split('.');
                        // First part is element name.
                        string name = subs[0];
                        // Second part is an attribute.
                        string attribute = subs[1];

                        // If the attribute is assigned to a value:
                        if (attribute.Contains("="))
                        {
                            string[] subAttributes = attribute.Split('=');
                            string   attributeName = subAttributes[0];
                            string   value         = subAttributes[1];
                            output.Add(new XElement(name, new XAttribute(attributeName, value)));
                            // If there is one more attribute, take a special route:
                            if (subs.Length == 3)
                            {
                                string extraAttribute = subs[2];
                                // This is the one that we want to get value from, so no value for this one.
                                string extraAttributeName = extraAttribute;
                                output.Last().Add(new XAttribute(extraAttributeName, ""));
                            }
                        }
                        // Add the element with valueless attribute.
                        else
                        {
                            output.Add(new XElement(name, new XAttribute(attribute, "")));
                        }
                    }
                    // Else just add the plain element to the output.
                    else
                    {
                        output.Add(new XElement(element));
                    }
                }
                return output;
            }

            /// <summary>
            /// Fill an XML file with existing map info.
            /// </summary>
            /// <param name="hardcardXML">Hardcard XML.</param>
            /// <param name="dctuXML">DCTU XML.</param>
            public static void FillXML(XDocument sourceXML, XDocument destinationXML)
            {
                // Add mapping pairs from the map file.
                AddMapping();
                foreach (Pair pair in pairs)
                {
                    string value = GetValue(sourceXML, pair.source);
                    if (value == null)
                    {
                        MessageBox.Show("Failed to get value from the source XML.");
                        Log.WriteLog("System - Failed to get value from the source XML.");
                        ClearMapping();
                        return;
                    }
                    PutValue(destinationXML, pair.destination, value);
                }
                ClearMapping();
            }

            /// <summary>
            /// Get a value from an XML.
            /// </summary>
            /// <param name="sourceXML">The XML to pull value from.</param>
            /// <param name="sourcePath">Source map.</param>
            /// <returns>The target value.</returns>
            private static string GetValue(XDocument sourceXML, List<XElement> sourcePath)
            {
                string value = "";
                XElement currentElement = sourceXML.Element(sourcePath[0].Name);
                // Check if we could get the main element.
                if (currentElement == null)
                {
                    Log.WriteLog("System - Could not get element \"" + sourcePath[0].Name + 
                        "\" in the source XML.");
                    MessageBox.Show("Could not get element \"" + sourcePath[0].Name +
                        "\" in the source XML.");
                    return null;
                }

                for (int i = 1; i < sourcePath.Count; i++)
                {
                    try
                    {
                        // If it has an attribute:
                        if (sourcePath[i].HasAttributes)
                        {
                            // Match attribute name and value.
                            if (sourcePath[i].FirstAttribute.Value == "")
                            {
                                currentElement = currentElement.Elements().
                                    Where(o => o.Name == sourcePath[i].Name
                                    && o.Attribute(sourcePath[i].FirstAttribute.Name) != null).ToArray()[0];
                            }
                            else
                            {
                                currentElement = currentElement.Elements().
                                    Where(o => o.Name == sourcePath[i].Name
                                    && o.Attribute(sourcePath[i].FirstAttribute.Name).Value
                                    == sourcePath[i].FirstAttribute.Value).ToArray()[0];
                            }
                        }
                        // No attribute:
                        else
                        {
                            currentElement = currentElement.Element(sourcePath[i].Name);
                        }
                    }
                    catch
                    {
                        // Error checking:
                        Log.WriteLog("System - Could not find a source XML element for element name: " +
                            sourcePath[i].Name + ".");
                        MessageBox.Show("Could not find a source XML element for element name: " +
                            sourcePath[i].Name + ".");
                        return null;
                    }
                }
                // Now put the value in the last element.
                XElement lastPath = sourcePath[sourcePath.Count - 1];
                // If it has an attribute to fill:
                if (lastPath.HasAttributes)
                {
                    try
                    {
                        value = currentElement.Attribute(lastPath.LastAttribute.Name).Value;
                    }
                    catch
                    {
                        Log.WriteLog("System - Map file's source is in wrong format.");
                        MessageBox.Show("Map file's source is in wrong format.");
                        return null;
                    }
                }
                // Or just fill the value of the element:
                else
                {
                    value = currentElement.Value;
                }
                // Succeed.
                Log.WriteLog("System - Successfully got value for element " + currentElement.Name + ".");
                return value;
            }

            /// <summary>
            /// Put a value in an XML.
            /// </summary>
            /// <param name="destinationXML">The XML to put value into.</param>
            /// <param name="destinationPath">Destination path.</param>
            /// <param name="value">The value to put into the XML.</param>
            private static void PutValue(XDocument destinationXML, List<XElement> destinationPath, string value)
            {
                XElement currentElement = destinationXML.Element(destinationPath[0].Name);
                // Check if we could get the main element.
                if (currentElement == null)
                {
                    Log.WriteLog("System - Could not get the main element in the destination XML.");
                    MessageBox.Show("Could not get the main element in the destination XML.");
                    return;
                }

                for (int i = 1; i < destinationPath.Count; i++)
                {
                    try
                    {
                        // If it has an attribute:
                        if (destinationPath[i].HasAttributes)
                        {
                            // Match attribute name and value.
                            currentElement = currentElement.Elements().
                                Where(o => o.Name == destinationPath[i].Name
                                && o.Attribute(destinationPath[i].FirstAttribute.Name).Value
                                == destinationPath[i].FirstAttribute.Value).ToArray()[0];
                        }
                        // No attribute:
                        else
                        {
                            currentElement = currentElement.Element(destinationPath[i].Name);
                        }
                    }
                    catch
                    {
                        Log.WriteLog("System - Could not find a destination XML element for element name: " +
                            destinationPath[i].Name + ".");
                        MessageBox.Show("Could not find a destination XML element for element name: " +
                            destinationPath[i].Name + ".");
                        return;
                    }
                }
                // Now put the value in the last element.
                XElement lastPath = destinationPath[destinationPath.Count - 1];
                // If it has an attribute to fill:
                if (lastPath.HasAttributes)
                {
                    currentElement.SetAttributeValue(lastPath.FirstAttribute.Name, value);
                }
                // Or just fill the value of the element:
                else
                {
                    try
                    {
                        currentElement.Value = value;
                    }
                    catch
                    {
                        Log.WriteLog("System - Wrong format for map destination.");
                        MessageBox.Show("Wrong format for map destination.");
                        return;
                    }
                }
                // Succeed.
                Log.WriteLog("System - Successfully put value for element " + currentElement.Name + ".");
            }
        }
