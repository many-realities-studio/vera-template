using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class VERALoggerTests
{
  private GameObject loggerGameObject;
  private VERALogger logger;
  private string testFilePath;
  private VERAColumnDefinition columnDefinition;

  [SetUp]
  public void SetUp()
  {
    // Create a new GameObject and attach the VERALogger component
    loggerGameObject = new GameObject();
    logger = loggerGameObject.AddComponent<VERALogger>();

    // Set up the test file path
    testFilePath = Path.Combine(Application.persistentDataPath, "test_log.csv");
    logger.filePath = testFilePath;
    Debug.Log("logger.filePath");
    // Create a dummy VERAColumnDefinition ScriptableObject
    columnDefinition = ScriptableObject.CreateInstance<VERAColumnDefinition>();
    columnDefinition.columns.Add(new VERAColumnDefinition.Column { name = "ts", type = VERAColumnDefinition.DataType.Date });
    columnDefinition.columns.Add(new VERAColumnDefinition.Column { name = "eventId", type = VERAColumnDefinition.DataType.Number });
    columnDefinition.columns.Add(new VERAColumnDefinition.Column { name = "data", type = VERAColumnDefinition.DataType.String });

    logger.columnDefinition = columnDefinition;

    // Initialize the logger
    logger.study_UUID = "test_study";
    logger.participant_UUID = "test_participant";
    logger.Initialize();
  }

  [TearDown]
  public void TearDown()
  {
    // Clean up the created files and GameObjects
    Object.DestroyImmediate(loggerGameObject);
    if (File.Exists(testFilePath))
    {
      File.Delete(testFilePath);
    }
    Object.DestroyImmediate(columnDefinition);
  }

  [UnityTest]
  public IEnumerator TestCreateEntryWithoutImmediateFlush()
  {
    // Create a log entry
    logger.CreateEntry(1, "TestData");

    // Verify the cache has the entry but the file is empty
    Assert.AreEqual(1, logger.cache.Count);
    Assert.IsFalse(File.Exists(testFilePath) && File.ReadAllLines(testFilePath).Length > 1);

    // Wait for a short duration
    yield return new WaitForSeconds(1);

    // Verify the file is still empty (no flush yet)
    Assert.AreEqual(1, logger.cache.Count);
    Assert.IsFalse(File.Exists(testFilePath) && File.ReadAllLines(testFilePath).Length > 1);
  }

  [UnityTest]
  public IEnumerator TestCreateEntryAndFlushBySizeLimit()
  {
    // Create multiple log entries to reach the cache size limit
    for (int i = 0; i < 100; i++)
    {
      logger.CreateEntry(i, $"TestData{i}");
    }
    // Verify the cache is now empty and the file has the entries
    Assert.AreEqual(0, logger.cache.Count);
    Assert.IsTrue(File.Exists(testFilePath));
    var lines = File.ReadAllLines(testFilePath);
    Assert.AreEqual(101, lines.Length); // Including header

    yield break;
  }

  [UnityTest]
  public IEnumerator TestCreateEntryAndFlushByTimeInterval()
  {
    // Create a single log entry
    logger.CreateEntry(1, "TestData");

    // Verify the cache has the entry but the file is empty
    Assert.AreEqual(1, logger.cache.Count);
    Assert.IsFalse(File.Exists(testFilePath) && File.ReadAllLines(testFilePath).Length > 1);

    // Wait for the flush interval to pass
    yield return new WaitForSeconds(6);

    // Verify the cache is empty and the file has the entry
    Assert.AreEqual(0, logger.cache.Count);
    Assert.IsTrue(File.Exists(testFilePath));
    var lines = File.ReadAllLines(testFilePath);
    Assert.AreEqual(2, lines.Length); // Including header
  }
}
