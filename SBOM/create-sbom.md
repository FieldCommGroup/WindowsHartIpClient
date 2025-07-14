Create a comprehensive Software Bill of Materials (SBOM) in SPDX 2.2 JSON format for this project. Follow
  these requirements:

   Use namespace http://fieldcommgroup.org/ns/2025/sbom/<windows-hartip-client>/<1.0> for the SBOM document. Use this EXACT DOCUMENT NAMESPACE. Do not change it.   

  1. **Document Structure**: Use proper SPDX 2.2 JSON schema with required fields:
     - spdxVersion: "SPDX-2.2"
     - dataLicense: "CC0-1.0"
     - SPDXID: "SPDXRef-DOCUMENT"
     - creationInfo with creators, created timestamp, and licenseListVersion
     - documentNamespace: unique URI for this SBOM
     - packages array with all components and dependencies

  2. **Analysis Requirements**:
     - Scan all source files, build files, and configuration files
     - Identify all direct and transitive dependencies from package managers, build systems, and makefiles        
     - Extract license information from LICENSE files, package metadata, and source file headers
     - Determine copyright holders and creation dates
     - Calculate file checksums where possible
     - Identify the latest git commit hash for version information

  3. **Package Information** (for each component):
     - name, SPDXID, versionInfo, downloadLocation
     - filesAnalyzed (true/false)
     - licenseConcluded, licenseDeclared, copyrightText
     - externalRefs for package manager references
     - supplier and originator information where available

  4. **File Information** (for key files):
     - fileName, SPDXID, checksums (SHA1, SHA256, MD5)
     - licenseConcluded, licenseInfoInFiles, copyrightText
     - Include at least: main source files, build files, license files, README files

  5. **Relationships**:
     - Use proper SPDX relationship types: DESCRIBES, DEPENDS_ON, CONTAINS, GENERATED_FROM
     - Document dependencies between packages
     - Show file-to-package relationships

  6. **External References**:
     - Include package manager references (npm, pip, maven, etc.)
     - Add repository URLs, security advisories, and documentation links
     - Use proper referenceCategory and referenceType

  7. **License Handling**:
     - Use SPDX license identifiers where possible
     - For non-standard licenses, include full license text in extractedLicensingInfo
     - Handle dual licenses and license expressions correctly

  8. **Output Requirements**:
     - Save as `<project-name>-sbom.spdx.json`
     - Ensure valid JSON structure with proper escaping
     - Include all required SPDX fields
     - Validate against SPDX 2.2 specification

  The SBOM should be comprehensive enough for supply chain security analysis, vulnerability scanning, and
  license compliance checking.