using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SOImporter : EditorWindow
{
    private Dictionary<string, int> columnMapping = new Dictionary<string, int>();
    private string[] headers;
    private string csvPreview = "";
    private Type selectedSOType;
    private string[] availableSOTypes;
    private int selectedTypeIndex = 0;
    private FieldInfo[] soFields;
    private string outputFolder = "Assets/ScriptableObjects/";

    [MenuItem("Tools/SO Importer")]
    public static void ShowWindow()
    {
        GetWindow<SOImporter>("SO Importer");
    }

    private void OnEnable()
    {
        RefreshSOTypes();
    }

    private void OnGUI()
    {
        GUILayout.Label("Universal ScriptableObject Importer", EditorStyles.boldLabel);

        // SO 타입 선택
        GUILayout.Space(10);
        GUILayout.Label("1. Select ScriptableObject Type:", EditorStyles.boldLabel);

        if (availableSOTypes != null && availableSOTypes.Length > 0)
        {
            int newIndex = EditorGUILayout.Popup("SO Type:", selectedTypeIndex, availableSOTypes);
            if (newIndex != selectedTypeIndex)
            {
                selectedTypeIndex = newIndex;
                selectedSOType = GetSOTypeByName(availableSOTypes[selectedTypeIndex]);
                RefreshFieldInfo();
                columnMapping.Clear();
            }
        }

        if (GUILayout.Button("Refresh SO Types"))
        {
            RefreshSOTypes();
        }

        // 출력 폴더 설정
        GUILayout.Space(10);
        GUILayout.Label("2. Output Folder:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolder = "Assets" + path.Substring(Application.dataPath.Length) + "/";
            }
        }
        GUILayout.EndHorizontal();

        // CSV 로드
        GUILayout.Space(10);
        GUILayout.Label("3. Load CSV:", EditorStyles.boldLabel);
        if (GUILayout.Button("Load CSV for Preview"))
        {
            LoadCSVPreview();
        }

        // CSV 미리보기 및 매핑
        if (headers != null && headers.Length > 0 && selectedSOType != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("4. CSV Preview:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(csvPreview, GUILayout.Height(100));

            GUILayout.Space(10);
            GUILayout.Label("5. Column Mapping:", EditorStyles.boldLabel);
            DrawColumnMapping();

            GUILayout.Space(10);
            if (GUILayout.Button("Import Data", GUILayout.Height(30)))
            {
                ImportData();
            }
        }
    }

    private void RefreshSOTypes()
    {
        List<string> soTypeNames = new List<string>();

        // TypeCache를 사용해서 ScriptableObject 타입들 찾기 (더 안정적)
        var soTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>();

        foreach (Type type in soTypes)
        {
            // 추상 클래스가 아니고, 제네릭이 아니며, SO_로 시작하는 것들만
            if (!type.IsAbstract &&
                !type.IsGenericType &&
                type.Name.StartsWith("SO_"))
            {
                soTypeNames.Add(type.Name);
                Debug.Log($"Found SO Type: {type.Name} in namespace: {type.Namespace}");
            }
        }

        // 만약 위 방법으로도 안되면 직접 어셈블리 검색
        if (soTypeNames.Count == 0)
        {
            Debug.Log("TypeCache method failed, trying assembly scan...");

            // Assembly-CSharp에서만 찾기 (사용자 코드가 있는 어셈블리)
            Assembly userAssembly = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "Assembly-CSharp")
                {
                    userAssembly = assembly;
                    break;
                }
            }

            if (userAssembly != null)
            {
                try
                {
                    foreach (Type type in userAssembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ScriptableObject)) &&
                            !type.IsAbstract &&
                            type.Name.StartsWith("SO_"))
                        {
                            soTypeNames.Add(type.Name);
                            Debug.Log($"Found SO Type in Assembly-CSharp: {type.Name}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error scanning Assembly-CSharp: {e.Message}");
                }
            }
        }

        availableSOTypes = soTypeNames.ToArray();

        Debug.Log($"Total SO types found: {availableSOTypes.Length}");
        foreach (string typeName in availableSOTypes)
        {
            Debug.Log($"Available SO: {typeName}");
        }

        if (availableSOTypes.Length > 0)
        {
            selectedSOType = GetSOTypeByName(availableSOTypes[0]);
            RefreshFieldInfo();
        }
    }

    private Type GetSOTypeByName(string typeName)
    {
        // TypeCache 사용
        var soTypes = TypeCache.GetTypesDerivedFrom<ScriptableObject>();

        foreach (Type type in soTypes)
        {
            if (type.Name == typeName && type.IsSubclassOf(typeof(ScriptableObject)))
            {
                return type;
            }
        }

        // TypeCache에서 못 찾으면 어셈블리 직접 검색
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName && type.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        return type;
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }

        return null;
    }

    private void RefreshFieldInfo()
    {
        if (selectedSOType == null) return;

        soFields = selectedSOType.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    private void LoadCSVPreview()
    {
        string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = ReadCSVWithEncoding(path);
        if (lines.Length == 0) return;

        headers = ParseCSVLine(lines[0]);

        // 미리보기 생성
        csvPreview = "";
        for (int i = 0; i < Mathf.Min(4, lines.Length); i++)
        {
            csvPreview += lines[i] + "\n";
        }

        SetupAutoMapping();
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField.Trim());
        return result.ToArray();
    }

    private void SetupAutoMapping()
    {
        columnMapping.Clear();

        if (soFields == null) return;

        foreach (FieldInfo field in soFields)
        {
            string fieldName = field.Name;

            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim().ToLower();
                string fieldLower = fieldName.ToLower();

                // 정확한 매치 또는 부분 매치
                if (header == fieldLower ||
                    header.Contains(fieldLower) ||
                    fieldLower.Contains(header))
                {
                    columnMapping[fieldName] = i;
                    break;
                }
            }
        }
    }

    private void DrawColumnMapping()
    {
        if (soFields == null) return;

        EditorGUILayout.BeginVertical("box");

        foreach (FieldInfo field in soFields)
        {
            // 시리얼라이즈되지 않는 필드는 제외
            if (field.IsStatic || field.IsInitOnly) continue;

            GUILayout.BeginHorizontal();

            // 필드 정보 표시
            string fieldLabel = $"{field.Name} ({field.FieldType.Name})";
            GUILayout.Label(fieldLabel, GUILayout.Width(200));

            int currentIndex = columnMapping.ContainsKey(field.Name) ? columnMapping[field.Name] : -1;

            List<string> options = new List<string> { "None" };
            options.AddRange(headers);

            int selectedIndex = currentIndex + 1;
            selectedIndex = EditorGUILayout.Popup(selectedIndex, options.ToArray());

            if (selectedIndex == 0)
            {
                if (columnMapping.ContainsKey(field.Name))
                    columnMapping.Remove(field.Name);
            }
            else
            {
                columnMapping[field.Name] = selectedIndex - 1;
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void ImportData()
    {
        string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = ReadCSVWithEncoding(path);
        int successCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            if (CreateSOAsset(values, i))
                successCount++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"Successfully imported {successCount} {selectedSOType.Name} assets!");
    }

    private bool CreateSOAsset(string[] values, int rowIndex)
    {
        try
        {
            ScriptableObject so = CreateInstance(selectedSOType);

            string assetName = $"{selectedSOType.Name}_{rowIndex}";
            string typeValue = "";

            // 필드 값 설정
            foreach (FieldInfo field in soFields)
            {
                if (!columnMapping.ContainsKey(field.Name)) continue;

                int columnIndex = columnMapping[field.Name];
                if (columnIndex >= values.Length) continue;

                string value = values[columnIndex].Trim();
                if (string.IsNullOrEmpty(value)) continue;

                SetFieldValue(so, field, value);

                // Type 필드값 저장
                if (field.Name.ToLower() == "type")
                {
                    typeValue = value;
                }
                // ID 필드가 있으면 파일명으로 사용
                else if (field.Name.ToLower().Contains("id") ||
                    field.Name.ToLower().Contains("name"))
                {
                    assetName = $"{selectedSOType.Name}_{value}";
                }
            }

            // Type 값이 있으면 _TYPE으로 끝나는 파일명 생성
            if (!string.IsNullOrEmpty(typeValue))
            {
                assetName = $"{selectedSOType.Name}_{typeValue}";
            }

            // 에셋 저장
            string assetPath = $"{outputFolder}{assetName}.asset";

            string directory = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(so, assetPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating {selectedSOType.Name} asset at row {rowIndex}: {e.Message}");
            return false;
        }
    }

    private void SetFieldValue(ScriptableObject so, FieldInfo field, string value)
    {
        Type fieldType = field.FieldType;

        try
        {
            if (fieldType == typeof(string))
            {
                field.SetValue(so, value);
            }
            else if (fieldType == typeof(int))
            {
                if (int.TryParse(value, out int intValue))
                    field.SetValue(so, intValue);
            }
            else if (fieldType == typeof(float))
            {
                if (float.TryParse(value, out float floatValue))
                    field.SetValue(so, floatValue);
            }
            else if (fieldType == typeof(bool))
            {
                if (bool.TryParse(value, out bool boolValue))
                    field.SetValue(so, boolValue);
                else
                {
                    string lowerValue = value.ToLower();
                    bool result = lowerValue == "1" || lowerValue == "y" ||
                                 lowerValue == "yes" || lowerValue == "true";
                    field.SetValue(so, result);
                }
            }
            else if (fieldType.IsEnum)
            {
                if (Enum.TryParse(fieldType, value, true, out object enumValue))
                    field.SetValue(so, enumValue);
            }
            // AssetReference 처리 - Addressable 주소만 지원
            else if (fieldType == typeof(AssetReference))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    AssetReference assetRef = CreateAssetReference(value);
                    if (assetRef != null)
                    {
                        field.SetValue(so, assetRef);
                    }
                }
            }
            // Vector2Int 처리
            else if (fieldType == typeof(Vector2Int))
            {
                string[] parts = value.Split(',');
                if (parts.Length >= 2 &&
                    int.TryParse(parts[0].Trim(), out int x) &&
                    int.TryParse(parts[1].Trim(), out int y))
                {
                    field.SetValue(so, new Vector2Int(x, y));
                }
            }
            else if (fieldType == typeof(Vector2))
            {
                string[] parts = value.Split(',');
                if (parts.Length >= 2 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y))
                {
                    field.SetValue(so, new Vector2(x, y));
                }
            }
            else if (fieldType == typeof(Vector3))
            {
                string[] parts = value.Split(',');
                if (parts.Length >= 3 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float z))
                {
                    field.SetValue(so, new Vector3(x, y, z));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to set field {field.Name} with value '{value}': {e.Message}");
        }
    }

    // 한글 호환을 위한 CSV 읽기 메서드
    private string[] ReadCSVWithEncoding(string path)
    {
        try
        {
            // BOM 확인을 통한 UTF-8 감지
            byte[] bom = new byte[3];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= 3)
                {
                    fs.Read(bom, 0, 3);
                    // UTF-8 BOM 확인 (EF BB BF)
                    if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                    {
                        Debug.Log("UTF-8 BOM 감지됨");
                        return File.ReadAllLines(path, System.Text.Encoding.UTF8);
                    }
                }
            }

            // 안전한 인코딩들만 시도
            List<System.Text.Encoding> encodings = new List<System.Text.Encoding>();

            // UTF-8 추가
            encodings.Add(System.Text.Encoding.UTF8);

            // EUC-KR 안전하게 추가
            try
            {
                encodings.Add(System.Text.Encoding.GetEncoding("EUC-KR"));
            }
            catch
            {
                Debug.Log("EUC-KR 인코딩을 사용할 수 없음");
            }

            // CP949 안전하게 추가
            try
            {
                encodings.Add(System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
            }
            catch
            {
                Debug.Log("ks_c_5601-1987 인코딩을 사용할 수 없음");
            }

            // 기본 인코딩 추가
            encodings.Add(System.Text.Encoding.Default);

            string[] bestResult = null;
            System.Text.Encoding bestEncoding = null;
            int bestScore = -1;

            foreach (var encoding in encodings)
            {
                try
                {
                    string[] lines = File.ReadAllLines(path, encoding);
                    if (lines.Length == 0) continue;

                    string testContent = string.Join("", lines);
                    int score = ScoreEncoding(testContent);

                    Debug.Log($"인코딩 {encoding.EncodingName} 점수: {score}");

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestResult = lines;
                        bestEncoding = encoding;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"인코딩 {encoding.EncodingName} 실패: {ex.Message}");
                    continue;
                }
            }

            if (bestResult != null && bestEncoding != null)
            {
                Debug.Log($"최적 인코딩 선택: {bestEncoding.EncodingName} (점수: {bestScore})");
                return bestResult;
            }

            // 최후의 수단: 바이트 단위로 읽어서 수동 변환
            Debug.LogWarning("모든 인코딩 실패. 수동 변환 시도");
            return ReadWithManualEncoding(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 파일 읽기 실패: {e.Message}");
            return new string[0];
        }
    }

    // 인코딩 품질 점수 계산
    private int ScoreEncoding(string content)
    {
        if (string.IsNullOrEmpty(content)) return -1000;

        int score = 0;
        int totalChars = 0;
        int brokenChars = 0;

        foreach (char c in content)
        {
            totalChars++;

            if (c >= 0xAC00 && c <= 0xD7AF) // 한글 완성형
            {
                score += 10;
            }
            else if (c >= 0x1100 && c <= 0x11FF) // 한글 자모
            {
                score += 5;
            }
            else if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)) // 정상 문자
            {
                score += 1;
            }
            else if (c == '�') // 명확히 깨진 문자
            {
                score -= 100;
                brokenChars++;
            }
            else if (c < 32 && c != '\t' && c != '\r' && c != '\n') // 제어 문자 (탭, 개행 제외)
            {
                score -= 10;
                brokenChars++;
            }
        }

        // 깨진 문자 비율이 높으면 점수 대폭 감소
        if (totalChars > 0)
        {
            float brokenRatio = (float)brokenChars / totalChars;
            if (brokenRatio > 0.1f) // 10% 이상 깨진 경우
            {
                score -= (int)(brokenRatio * 1000);
            }
        }

        return score;
    }

    // 수동 인코딩 변환
    private string[] ReadWithManualEncoding(string path)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(path);

            // EUC-KR로 시도
            try
            {
                var eucKr = System.Text.Encoding.GetEncoding("EUC-KR");
                string content = eucKr.GetString(bytes);
                if (!content.Contains("�")) // 깨진 문자가 없으면
                {
                    Debug.Log("수동 EUC-KR 변환 성공");
                    return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"수동 EUC-KR 변환 실패: {ex.Message}");
            }

            // UTF-8로 시도
            try
            {
                string content = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("수동 UTF-8 변환 시도");
                return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                Debug.Log($"수동 UTF-8 변환 실패: {ex.Message}");
            }

            // 기본 인코딩으로 시도
            try
            {
                string content = System.Text.Encoding.Default.GetString(bytes);
                Debug.Log("기본 인코딩 변환 시도");
                return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                Debug.Log($"기본 인코딩 변환 실패: {ex.Message}");
            }

            return new string[0];
        }
        catch (Exception ex)
        {
            Debug.LogError($"수동 인코딩 변환 실패: {ex.Message}");
            return new string[0];
        }
    }

    // 한글이 포함되어 있는지 확인하는 메서드
    private bool ContainsKorean(string text)
    {
        foreach (char c in text)
        {
            // 한글 유니코드 범위: AC00-D7AF (완성형), 1100-11FF (자모)
            if ((c >= 0xAC00 && c <= 0xD7AF) || (c >= 0x1100 && c <= 0x11FF))
            {
                return true;
            }
        }
        return false;
    }

    // AssetReference 생성 메서드 - Addressable 주소만 지원
    private AssetReference CreateAssetReference(string addressableAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(addressableAddress))
            {
                return null;
            }

            // Addressable 주소로 GUID 찾기
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                foreach (var group in settings.groups)
                {
                    foreach (var entry in group.entries)
                    {
                        if (entry.address == addressableAddress)
                        {
                            Debug.Log($"AssetReference 생성 성공: {addressableAddress} -> {entry.guid}");
                            return new AssetReference(entry.guid);
                        }
                    }
                }
            }

            Debug.LogWarning($"Addressable 주소를 찾을 수 없음: {addressableAddress}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"AssetReference 생성 실패: {addressableAddress} - {e.Message}");
            return null;
        }
    }
}