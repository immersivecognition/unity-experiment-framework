"""
Python3 script used to generate UXF .md documentation from a .xml documentation tree 
"""

import markdown_generator as mg
import xml.etree.ElementTree as ET
import sys
import os


def inline_code(text):
    return "`" + text + "`"
        

class DocParser(object):

    def __init__(self, type_name, type_member):
        self.type_name = type_name
        self.type_member = type_member
        self.fields = []
        self.properties = []
        self.methods = []
        

    def add_child(self, symbol_type, child_member):
        if symbol_type == "M":
            self.methods.append(child_member)
        elif symbol_type == "F":
            self.fields.append(child_member)
        elif symbol_type == "P":
            self.properties.append(child_member)



    def to_md(self, directory):
        md_path = os.path.join(directory, f"{self.type_name}.md")
        print(f"Writing {md_path}")
        with open(md_path, 'w') as f:
            writer = mg.Writer(f)

            full_name = symbol_from_member(self.type_member)
            writer.write_heading(inline_code(full_name), 4)
        
            writer.writeline(mg.emphasis(summary_from_member(self.type_member)))
            
            writer.write_hrule()
            
            # FIELDS

            writer.write_heading("Fields", 3)
            if len(self.fields) == 0:
                writer.writeline(mg.emphasis("None"))
            else:
                for field in self.fields:
                    symbol = symbol_from_member(field)
                    
                    short_name = symbol.replace(full_name + ".", "")

                    writer.writeline(
                        inline_code(short_name) + ": " + \
                        summary_from_member(field))

            # PROPERTIES

            writer.write_heading("Properties", 3)
            if len(self.properties) == 0:
                writer.writeline(mg.emphasis("None"))
            else:
                for prop in self.properties:
                    symbol = symbol_from_member(prop)

                    short_name = symbol.replace(full_name + ".", "")

                    writer.writeline(
                        inline_code(short_name) + ": " +
                        summary_from_member(prop))

            # METHODS

            writer.write_heading("Methods", 3)
            if len(self.methods) == 0:
                writer.writeline(mg.emphasis("None"))
            else:
                for method in self.methods:
                    symbol = symbol_from_member(method)

                    symbol = symbol.replace("#ctor", full_name)
                    symbol = symbol.replace(",", ", ")
                    symbol = symbol.replace("{", "<")
                    symbol = symbol.replace("}", ">")
                    symbol = symbol.replace("System.String", "string")
                    symbol = symbol.replace("System.Int32", "int")
                    symbol = symbol.replace("System.Single", "float")
                    symbol = symbol.replace("System.Object", "object")

                    if not symbol[-1] == ")":
                        symbol += "()"

                    writer.writeline(inline_code(symbol))
                    blockquote = mg.BlockQuote()
                    blockquote.append(summary_from_member(method))
                    blockquote.append("")
                    blockquote.append(mg.strong("Parameters"))
                    blockquote.append("")
                    params = [p for p in method if p.tag == "param"]

                    pre = "* "
                    if len(params) == 0:
                        blockquote.append(mg.emphasis("None"))
                    else:
                        for param in params:
                            param_name = param.attrib["name"]
                            param_text = param.text if param.text is not None else ""
                            blockquote.append(pre + inline_code(
                                param_name) + ": " + param_text)
                            blockquote.append("")
                    writer.write(blockquote)


            writer.write_hrule()
            writer.writeline(mg.emphasis("Note: This file was automatically generated"))



            

def symbol_from_member(member):
    return member.attrib["name"].split(":")[1]
    
def summary_from_member(member):
    return member[0].text.strip()


def write_types_list(types_dict, location):
    with open(location, 'w') as f:
        writer = mg.Writer(f)
        for type_name in types_dict:
            member = types_dict[type_name].type_member
            full_name = symbol_from_member(member)
            writer.write_heading(f"[[{full_name}|{type_name}]]", 4)
            writer.writeline(summary_from_member(member))

        writer.writeline(mg.emphasis(
            "Note: This file was automatically generated"))


if __name__ == "__main__":
    filepath = sys.argv[1]
    tree = ET.parse(filepath)
    root = tree.getroot()
    
    types_dict = {}

    # find all Types
    for child in root[1]:
        if child.tag != "member":
            continue
        member_name = child.attrib['name']
        symbol_type, symbol = member_name.split(":")
        symbol_path = symbol.split(".")
        if symbol_type != "T" or len(symbol_path) == 1 or symbol_path[0] != "UXF":
            continue
        type_name = symbol_path[1]
        types_dict[type_name] = DocParser(type_name, child)

    # add Methods, Fields and Properties to Types
    for child in root[1]:
        if child.tag != "member":
            continue
        member_name = child.attrib['name']
        symbol_type, symbol = member_name.split(":")
        symbol_path = symbol.split(".")
        if (symbol_type == "T"
            or len(symbol_path) == 1
            or symbol_path[0] != "UXF"):
            continue

        associated_class = symbol_path[1]
        if associated_class in types_dict:
            types_dict[associated_class].add_child(symbol_type, child)
    
    write_types_list(types_dict, "wiki/Generated/Classes.md")

    for type_name in types_dict:
        types_dict[type_name].to_md("wiki/Generated")

