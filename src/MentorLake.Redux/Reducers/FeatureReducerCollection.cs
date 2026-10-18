namespace MentorLake.Redux.Reducers;

/// <summary>
/// A named collection of feature reducers, produced by <see cref="IReducerFactory.Create"/>.
/// </summary>
public class FeatureReducerCollection : List<IFeatureReducer>
{

}
